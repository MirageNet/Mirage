# Change Scene Example

This example demonstrates how to change between different scenes and how to load and unload additive scenes in Mirage.

![ChangeScene Example](/img/examples/change-scene/ChangeScene.png)

## Setup

1.  Open the `Assets/Examples/ChangeScene/Scenes` folder.
2.  Add the following scenes to your Build Settings (File > Build Settings) in any order:
    -   `Main.unity`
    -   `Room1.unity`
    -   `Room2.unity`
    -   `Additive.unity`

## Key Components

### `SceneSwitcherHud.cs`

This script is attached to a GameObject in the `Main.unity` scene and provides the UI for switching between scenes. It uses the `NetworkSceneManager` to handle scene loading and unloading.

-   **`Room1ButtonHandler()` and `Room2ButtonHandler()`:** These methods demonstrate how to perform a normal scene change using `sceneManager.ServerLoadSceneNormal()`. When these buttons are clicked, the server loads the specified scene, and all connected clients will also switch to that scene.
-   **`AdditiveButtonHandler()`:** This method toggles between loading and unloading an additive scene. It uses `sceneManager.ServerLoadSceneAdditively()` to load the `Additive.unity` scene on top of the currently active scene, and `sceneManager.ServerUnloadSceneAdditively()` to unload it. This shows how to manage multiple scenes simultaneously.

## How to Run

1.  **Run as Host:** Open `Main.unity` in the Unity Editor and press Play. The game will start as a host (server + client).
2.  **Run as Client:** Build and run a standalone instance of the game. In the standalone instance, click the "Client" button to connect to the host running in the editor.
3.  Use the buttons in the UI to switch between `Room1`, `Room2`, and to load/unload the `Additive` scene. Observe how the scenes change for both the host and the client.

This example highlights the flexibility of Mirage's scene management, allowing for dynamic loading and unloading of scenes to create complex game worlds.