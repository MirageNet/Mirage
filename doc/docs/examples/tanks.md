---
sidebar_position: 8
---

# Tanks Example

The Tanks example is a small multiplayer arena game. Each player drives a tank, enters a name, clicks **Ready**, and then shoots the other tanks. The last tank standing wins.

It shows a complete game loop built from a few small scripts: spawning players at spawn points, ready-up with a `[ServerRpc]`, server-spawned projectiles, server-side hit detection, and SyncVars for health and score.

<!-- TODO screenshot: Tanks gameplay, 2-3 tanks in the arena with name labels and the health/score HUD -->

## How It Works

### `PlayerSpawner`
`PlayerSpawner` spawns a tank for each player as soon as they are authenticated. It is a copy of [`NetworkSceneLoader`](/docs/guides/scene-loading/network-scene-loader) with scene changing removed (players stay in the starting scene) and spawn points added:

- **Immediate Client Readiness**: The client is already in the starting scene, so it is marked ready on authentication (`player.SceneIsReady = true`).
- **Spawn Points**: Each new tank is placed at the next transform in the `SpawnPoints` list, looping back to the first one.

### `Tank`
Each player controls one `Tank`. The tank's game state is stored in SyncVars, which the server sets and clients read:

```csharp
[SyncVar] public int health;
[SyncVar] public int score;
[SyncVar] public string playerName;
[SyncVar] public bool allowMovement;
[SyncVar] public bool isReady;
```

- **Movement**: Only the local player reads input (`IsLocalPlayer`). Horizontal input turns the tank, and forward input drives it with a `NavMeshAgent`. The position is synced to other players by `NetworkTransform`.
- **Ready Up**: The ready button calls `SendReadyToServer`, which sends the chosen name to the server with a `[ServerRpc]`. The server sets `playerName` and `isReady`, and those SyncVars reach every client.
- **Shooting**: Pressing **Space** calls `CmdFire` on the server. The server spawns the projectile, then calls `RpcOnFire` so every client plays the shoot animation:

```csharp
// this is called on the server
[ServerRpc]
private void CmdFire()
{
    var projectile = Instantiate(projectilePrefab, projectileMount.position, transform.rotation);
    var projectileComp = projectile.GetComponent<Projectile>();
    projectileComp.source = gameObject;
    ServerObjectManager.Spawn(projectile);
    RpcOnFire();
}

// this is called on the tank that fired for all observers
[ClientRpc]
private void RpcOnFire()
{
    animator.SetTrigger("Shoot");
}
```

### `Projectile`
Projectiles are spawned by the server, but their movement is not synced:

- **Simulated on both sides**: In `Start`, every copy of the projectile (server and clients) adds the same forward force. Because they start in the same place with the same force, they move the same way without sending position updates.
- **Server-side hits**: `OnTriggerEnter` only runs its logic on the server (`[Server(error = false)]`). When it hits another tank, the server lowers that tank's `health` and adds to the shooter's `score`. Both are SyncVars, so the change reaches every client.
- **Lifetime**: The server destroys the projectile after `destroyAfter` seconds, or when it hits something, with `ServerObjectManager.Destroy`. That removes it on all clients too.

### `TankGameManager`
`TankGameManager` runs the match UI on each instance:

- **Ready Check**: Once at least `MinimumPlayersForGame` tanks exist and all of them are ready, the game starts: tank movement is allowed and the health/score labels are shown.
- **Game Over**: When only one tank is still alive (and there is more than one player), the game over panel shows the winner's name.
- **Single player**: With one player the game never ends, so you can test movement and shooting on your own.

:::note
After a game ends there is no rematch. Stop and start the host to play again.
:::

## Running the Example

1. Import the **Tanks** sample, then open `Scenes/Scene.unity`.
2. Press **Play** in the Editor and click **Host (Server + Client)** in the HUD.
3. Start one or more built instances and click **Client**.
4. Enter a name in each instance and click **Ready**. The game starts when every player is ready.
5. Use **W** and **A**/**D** (or the arrow keys) to drive and turn, and **Space** to fire.

<!-- TODO screenshot: the start panel with the name field and Ready button -->
