---
sidebar_position: 2
---

# Coin Grab Arena Example

Coin Grab Arena is a small, complete multiplayer game built with Mirage. Players control colored capsules in a top-down arena and race to collect coins before the round timer expires or someone reaches the target score.

It is designed as the next step after the [Basic](/docs/examples/basic) example, showing how to combine player spawning, SyncVar hooks, `NetworkTime` countdowns, server-authoritative pickups, `[ClientRpc]` effects, and a round state machine into a full game loop.

<!-- TODO screenshot: Coin Grab Arena gameplay with 2-3 colored player capsules, normal and gold coins, and the scoreboard HUD -->

## How It Works

### `CoinGrabSpawner`
`CoinGrabSpawner` spawns a player capsule as soon as a client authenticates (`Server.Authenticated`). Because the entire game runs in a single scene, the client is marked ready immediately (`player.SceneIsReady = true`).

Initial SyncVars (`PlayerName`, `Color`, and `Score`) are assigned **before** calling `ServerObjectManager.AddCharacter` so they are serialized directly into the initial `SpawnMessage`:

```csharp
private void SpawnCharacterForPlayer(INetworkPlayer player)
{
    Debug.Assert(player.Identity == null, "Player already has a character spawned.");

    GetNextSpawnPoint(out var position, out var rotation);
    var character = Instantiate(PlayerPrefab, position, rotation);

    // Set initial SyncVars before AddCharacter so they are packed into the initial SpawnMessage
    // instead of sending a separate update packet on the next frame
    var playerNumber = _nextPlayerNumber;
    _nextPlayerNumber++;

    character.PlayerName = $"Player {playerNumber}";
    character.Color = PlayerColors[(playerNumber - 1) % PlayerColors.Length];
    character.Score = 0;

    ServerObjectManager.AddCharacter(player, character.gameObject);
}
```

### `CoinGrabPlayer`
Each player prefab has a `CoinGrabPlayer` component that syncs its display name, capsule color, and score:

```csharp
[SyncVar(hook = nameof(OnNameChanged)), MaxLength(20)]
public string PlayerName;

[SyncVar(hook = nameof(OnColorChanged))]
public Color32 Color;

[SyncVar(hook = nameof(OnScoreChanged))]
public int Score;
```

- **Owner-Authoritative Movement**: Only the owning client enables its `CharacterController` (`OnStartLocalPlayer`) and reads `Input.GetAxis` in `Update`. Position changes are synced to the server and other clients via `NetworkTransform` with `ClientAuthority = true`. This keeps movement responsive for the local player while the server retains authority over coin pickups and scoring.
- **Server-Only Collider**: A separate non-trigger `CapsuleCollider` (`ServerCollider`) on the player is enabled only on the server (`OnStartServer`) along with a kinematic `Rigidbody` so `OnTriggerEnter` events on the coin's trigger collider only fire on the server.
- **Round Reset Teleport**: Because the owning client has transform authority, the server resets positions at the start of a round by updating its own transform and calling `[ClientRpc] RpcTeleport` so the owner and observers snap immediately to their spawn points.
- **Validated Name Change**: Players can change their display name via a `[ServerRpc]` protected by `[RateLimit]` and `[MaxLength(20)]`. If a client sends an empty string or control characters, the server rejects it and flags the player with `Owner.SetError`:

```csharp
[ServerRpc]
[RateLimit(Interval = 2f, Refill = 1, MaxTokens = 3, Penalty = 5)]
public void CmdSetPlayerName([MaxLength(20)] string requestedName)
{
    if (string.IsNullOrWhiteSpace(requestedName))
    {
        Owner.SetError(2, PlayerErrorFlags.CustomError);
        return;
    }

    var trimmed = requestedName.Trim();

    // Reject control characters so malicious clients cannot break the scoreboard layout
    for (var i = 0; i < trimmed.Length; i++)
    {
        if (char.IsControl(trimmed[i]))
        {
            Owner.SetError(5, PlayerErrorFlags.LikelyCheater);
            return;
        }
    }

    PlayerName = trimmed;
}
```

### `Coin`
Coins are spawned by the server with two `[SyncVar(initialOnly = true)]` fields—`Value` and `IsGold`—set before `ServerObjectManager.Spawn` so clients know immediately whether to display a normal coin (1 point) or a larger gold coin (5 points).

Pickup logic runs only on the server (`[Server(error = false)]` on `OnTriggerEnter`):

- **Why the server decides pickups**: If clients detected their own pickups, two players with latency could both think they collected the same coin, or a modified client could claim coins from across the map.
- **Anti-Cheat Distance Check**: Before awarding points, the server verifies that the player is within `MaxPickupDistance` of the coin, calling `player.Owner?.SetError(5, PlayerErrorFlags.LikelyCheater)` if not.
- **Single-Pickup Guard**: A `_collected` boolean prevents two players who touch the same coin in the same physics step from both scoring before `ServerObjectManager.Destroy` removes the object.
- **Pickup Effect**: The server calls `[ClientRpc] RpcPlayPickupEffect` with the coin's position before destroying the coin so every client sees the particle burst.

```csharp
[Server(error = false)]
private void OnTriggerEnter(Collider other)
{
    if (_collected)
        return;

    // Ignore pickups outside the active Playing state (e.g. if a round just ended this frame)
    if (_gameManager.State != RoundState.Playing)
        return;

    if (!other.TryGetComponent<CoinGrabPlayer>(out var player))
        return;

    // Server-side distance validation protects against position desync or spoofed transforms
    var sqrDistance = (player.transform.position - transform.position).sqrMagnitude;
    if (sqrDistance > MaxPickupDistance * MaxPickupDistance)
    {
        player.Owner?.SetError(5, PlayerErrorFlags.LikelyCheater);
        return;
    }

    // Flip flag immediately so a second player touching the coin in the same physics step is ignored
    _collected = true;

    player.Score += Value;

    // Notify all clients to play the pickup burst before destroying the networked coin object
    RpcPlayPickupEffect(transform.position, IsGold);

    _spawner.OnCoinCollected(this);
    _gameManager.OnPlayerScored(player);

    ServerObjectManager.Destroy(gameObject);
}
```

### `CoinSpawner`
`CoinSpawner` runs on the server and spawns up to `MaxCoins` on an interval while the game is in `RoundState.Playing`. Before spawning a coin, it tests random candidate positions with `Physics.CheckSphere` against `ObstacleLayer` (`QueryTriggerInteraction.Ignore`) so coins never spawn inside walls or arena pillars (which are placed on the layer configured in `ObstacleLayer`). When a round ends or the server stops, `ClearAllCoins` destroys any remaining coins.

### `CoinGrabGameManager`
`CoinGrabGameManager` is a scene `NetworkBehaviour` that drives the round state machine on the server:

1. **`Waiting`**: Fewer than `MinPlayers` are connected. Players can move around the arena while waiting.
2. **`Countdown`**: Resets all player scores to `0`, teleports everyone to spawn points, and counts down for 3 seconds.
3. **`Playing`**: `CoinSpawner` spawns coins while the round timer counts down. Ends when a player reaches `TargetScore` or time expires.
4. **`Results`**: Clears all coins, announces the winner (or tie) via `WinnerText`, and displays the final scoreboard for 5 seconds before starting the next countdown.

Instead of syncing a timer float every frame, the server sets `[SyncVar] public double RoundEndTime` once when entering a timed state using `NetworkTime.Time`. Clients compute the remaining time locally:

```csharp
public float GetRemainingTime()
{
    if (!IsServer && !IsClient)
        return 0f;

    return Mathf.Max(0f, (float)(RoundEndTime - NetworkTime.Time));
}
```

### `CoinGrabHud`
`CoinGrabHud` is a regular `MonoBehaviour` with no networking logic. It subscribes to `World.onSpawn` / `World.onUnspawn` to track active `CoinGrabPlayer` instances and rebuilds the sorted scoreboard only when a player's `OnStatsChanged` event fires. The countdown label only updates when the whole-second value changes, avoiding per-frame string allocations.

## Running the Example

1. Import the **Coin Grab Arena** sample from the Package Manager, then open `Scenes/CoinGrabArena.unity`.
2. Press **Play** in the Editor and click **Host (Server + Client)** in the HUD.
   - To test a full round by yourself in Host mode, set **Min Players** to `1` on the `GameManager` object in the Inspector.
3. Start one or more built instances and click **Client**.
4. Use **WASD** or the **Arrow Keys** to move around and collect coins.
