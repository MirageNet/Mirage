---
sidebar_position: 3
---

# Pong Example

Pong is a 2-player multiplayer game demonstrating server-authoritative 2D physics, dynamic paddle spawning, and automatic ball lifecycle management using Mirage.

![Pong](/img/examples/pong/Pong1.jpg)

## How It Works

### `PaddleSpawner`
Spawning is handled by `PaddleSpawner`, which follows Mirage's event-driven spawner pattern:

- **Immediate Client Readiness**: In single-scene setups without scene loading transitions, clients are marked ready immediately upon authentication (`player.SceneIsReady = true`).
- **Slot Selection**: Tracks which player owns each side. A new player takes the left side if it is free, otherwise the right side. Extra players can watch but don't get a paddle.
- **Match Lifecycle & Ball Spawning**: Once both sides have a paddle, `PaddleSpawner` instantiates the ball prefab and spawns it over the network via `ServerObjectManager.Spawn()`.
- **Match Reset**: When a paddle player disconnects, their side is freed and `PaddleSpawner` destroys the ball on the server to reset the match.

```csharp
private void SpawnCharacterForPlayer(INetworkPlayer player)
{
    Debug.Assert(player.Identity == null, "Player already has a character spawned.");

    Transform start;
    if (_leftPlayer == null)
    {
        _leftPlayer = player;
        start = leftRacketSpawn;
    }
    else if (_rightPlayer == null)
    {
        _rightPlayer = player;
        start = rightRacketSpawn;
    }
    else
    {
        // both sides taken, extra players can watch but don't get a paddle.
        // AddCharacter normally sends existing objects, so without a character we need to send them ourselves
        ServerObjectManager.SpawnVisibleObjects(player, ignoreHasCharacter: true);
        return;
    }

    var character = Instantiate(PlayerPrefab, start.position, start.rotation);
    ServerObjectManager.AddCharacter(player, character.gameObject);

    // Serve the ball once both sides have a paddle
    if (_leftPlayer != null && _rightPlayer != null)
    {
        _ball = Instantiate(ballPrefab);
        ServerObjectManager.Spawn(_ball);
    }
}
```

### `Ball` & Server-Authoritative Physics
The ball is simulated entirely by Unity's 2D physics engine on the server:

- **Server-Only Simulation**: `Ball.OnStartServer` sets `rigidbody2d.simulated = true` and launches the ball with an initial horizontal velocity. Physics simulation remains disabled on clients.
- **Deflection & Hit Factor**: When the ball collides with a player paddle on the server (`OnCollisionEnter2D`), it calculates the vertical contact offset to adjust angle and velocity.
- **Network Synchronization**: The ball's position is sent to clients by a server-authority `NetworkTransform`. Clients don't simulate the ball, they only show where the server says it is.

### `Player` Racket Controls
- `Player.cs` runs movement in `FixedUpdate()`, checking `IsLocalPlayer` so inputs only control the local paddle.
- The racket's `NetworkTransform` has **Client Authority** enabled, so the owner moves its own paddle and the position is sent to the server and the other client. This keeps the paddle responsive, at the cost of trusting the client with its paddle position.

:::note
There is no scoring: the ball bounces off all four walls. The sample focuses on spawning and syncing, and is a good starting point for adding goals and a score.
:::

## Running the Example

1. Import the **Pong** sample, then open `Scenes/Scene.unity`.
2. Press **Play** in the Editor and click **Host (Server + Client)** in the HUD. The left racket will spawn.
3. Start a built client instance and click **Client**. The right racket will spawn and the ball will serve towards the players.
4. Use **W/S** or **Up/Down Arrow** keys to move your racket.
