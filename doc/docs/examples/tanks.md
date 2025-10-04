# Tanks Example

This example demonstrates a multiplayer tank game with networked movement, projectile firing, health, and scoring. It showcases how to synchronize game state, handle player input, and manage game logic across the network.

## Setup

1.  Open the `Assets/Examples/Tanks/Scenes/Scene.unity` scene.
2.  Ensure this scene is added to your Build Settings (File > Build Settings).

## Key Components

### `Projectile.cs`

This script is attached to the projectile prefab. It handles the projectile's behavior after being fired:

-   **Movement:** The projectile moves forward based on its initial force.
-   **Server-side Collision:** Collision detection and damage application are handled on the server. When a projectile hits a player (another tank), it reduces the target tank's health and increases the firing tank's score.
-   **Networked Destruction:** The projectile is destroyed on the server after a set time or upon collision, and this destruction is synchronized to all clients.

### `Tank.cs`

This script is attached to the tank player prefab and manages individual tank behavior:

-   **Movement:** Handles player input for tank rotation and movement using a `NavMeshAgent`.
-   **Firing:** Detects player input to fire projectiles. The `CmdFire` method (a `[ServerRpc]`) is called from the client to instruct the server to spawn a projectile. The `RpcOnFire` method (a `[ClientRpc]`) is then called by the server on all clients to trigger the firing animation.
-   **Health and Score:** Uses `SyncVar` to synchronize `health`, `score`, `playerName`, `allowMovement`, and `isReady` across the network. These values are updated on the server and automatically propagated to clients.
-   **Player Ready State:** Includes methods (`SendReadyToServer`, `CmdReady`) to manage a player's ready state, which is used by the `TankGameManager` to determine when the game can start.

### `TankGameManager.cs`

This script is a central manager for the Tanks game, typically placed on a non-networked GameObject in the scene. It oversees the overall game flow:

-   **Player Management:** Tracks connected players (tanks) and their ready states.
-   **Game State:** Manages the game's state, including `IsGameReady` and `IsGameOver`.
-   **UI Updates:** Updates the UI to display player health, score, and game status.
-   **Game Start/End:** Initiates the game when enough players are ready and handles game over conditions (e.g., when only one player remains alive).
-   **Movement Control:** Enables or disables tank movement based on the game's state.

## How to Run

1.  **Run as Host:** Press Play in the Unity Editor. The game will start as a host (server + client).
2.  **Run as Client:** Build and run a standalone instance of the game. In the standalone instance, click the "Client" button to connect to the host running in the editor.
3.  Enter a player name and click "Ready". Once all players are ready, the game will start. Control your tank using the arrow keys or WASD, and fire projectiles with the Spacebar. Observe how health and score are synchronized across all connected clients.

This example provides a comprehensive demonstration of building a networked action game with Mirage, covering essential aspects like player control, projectile physics, and game state management.