# Basic Example

This example demonstrates fundamental networking concepts in Mirage, including spawning players, synchronizing data using `SyncVar`, and basic UI interaction.

![Basic Example](/img/examples/basic/Basic.PNG)

![Basic Player](/img/examples/basic/BasicPlayer.PNG)

## Setup

1.  Open the `Assets/Examples/Basic/Scenes/Example.unity` scene.
2.  Ensure this scene is added to your Build Settings (File > Build Settings).

## Key Components

### `CanvasCharacterSpawner.cs`

This script, derived from `CharacterSpawner`, is responsible for spawning player prefabs. It customizes the spawning process to make the spawned player objects children of a specified UI `Transform` (e.g., a Canvas panel). This allows the player representations to be displayed within a Unity UI canvas.

### `Player.cs`

This script is attached to the player prefab and demonstrates:

-   **`SyncVar` Synchronization:** It uses `SyncVar` attributes to synchronize `playerNo` (player number), `playerColor` (a random color assigned by the server), and `playerData` (a random integer updated periodically by the server).
-   **Network Callbacks:** It utilizes `OnStartServer`, `OnStartClient`, and `OnStartLocalPlayer` callbacks to initialize player-specific data and UI elements:
    -   `OnStartServer`: Assigns a unique player number and a random color on the server.
    -   `OnStartClient`: Positions the player's UI element on the canvas and applies the synchronized player number and color.
    -   `OnStartLocalPlayer`: Applies a visual highlight to the local player's UI element.
-   **Server-only Logic:** The `UpdateData` method, marked with `[Server(error = false)]`, runs only on the server to update `playerData` periodically.

## How to Run

1.  **Run as Host:** Press Play in the Unity Editor. The game will start as a host (server + client).
2.  **Run as Client:** Build and run a standalone instance of the game (File > Build And Run). In the standalone instance, click the "Client" button to connect to the host running in the editor.
3.  Observe how new player UI elements appear on both the host and client, and how their data (`playerData`) updates synchronously.

This example provides a solid foundation for understanding how to set up basic multiplayer interactions and data synchronization in Mirage.