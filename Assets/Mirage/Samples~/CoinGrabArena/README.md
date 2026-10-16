# Coin Grab Arena Example

A top-down multiplayer arena game where players run around collecting coins and the highest score when the round ends wins.

See the [Coin Grab Arena example docs](https://miragenet.github.io/Mirage/docs/examples/coin-grab-arena) for a full walkthrough of how the scripts work.

## Setup

No Build Settings changes are needed because the sample runs in a single scene (`Scenes/CoinGrabArena.unity`). Arena walls and pillars are placed on the layer configured in `CoinSpawner.ObstacleLayer` so coins only spawn in open space.

## How to Play

1. Open `Scenes/CoinGrabArena.unity` in the Editor.
2. Press **Play** and click **Host (Server + Client)** in the network HUD.
   - To test alone as Host without waiting for a second player, select the `GameManager` object in the scene and set **Min Players** to `1`.
3. Build and run one or more client instances (2-8 players) and click **Client** on each.
4. Once enough players have joined, a 3-second countdown starts and coins begin spawning around the arena:
   - **Normal Coins** (yellow) are worth **1 point**.
   - **Gold Coins** (orange, larger, 10% spawn chance) are worth **5 points**.
5. Use **WASD** or the **Arrow Keys** to move around and collect coins. You can also enter a custom name in the top bar and click **Set Name**.
6. The round ends when a player reaches the target score (default `30`) or the round timer (`120s`) runs out. The winner and scoreboard are shown for 5 seconds before the next round starts automatically.

## How It Works

- **`CoinGrabSpawner`**: Spawns a `CoinGrabPlayer` capsule at the next spawn point as soon as a player authenticates (`Server.Authenticated`), setting initial `PlayerName`, `Color`, and `Score` SyncVars before calling `ServerObjectManager.AddCharacter`.
- **`CoinGrabPlayer`**: Handles local WASD movement using `CharacterController` and `NetworkTransform` (`ClientAuthority = true`), syncs player name, color, and score via `[SyncVar]` hooks, teleports back to spawn points on round reset via `[ClientRpc]`, and validates custom names in a rate-limited `[ServerRpc]`.
- **`Coin`**: Represents a spawned coin with `[SyncVar(initialOnly = true)]` fields for value and gold status. Pickup detection runs exclusively on the server via `OnTriggerEnter` with server-side distance validation and a `[ClientRpc]` pickup particle burst before `ServerObjectManager.Destroy`.
- **`CoinSpawner`**: Server-only component that spawns up to `MaxCoins` at random obstacle-free positions (`Physics.CheckSphere`) on an interval during the `Playing` state and clears all coins when the round ends or the server stops.
- **`CoinGrabGameManager`**: Scene `NetworkBehaviour` that runs the server-authoritative round state machine (`Waiting` -> `Countdown` -> `Playing` -> `Results`) and syncs `RoundEndTime` using `NetworkTime.Time` so clients compute the countdown locally without per-frame network traffic.
- **`CoinGrabHud`**: Pure UI `MonoBehaviour` that listens to `NetworkWorld` spawn events and player/game-manager state events to render the round state, countdown timer, and sorted scoreboard without per-frame allocations.
