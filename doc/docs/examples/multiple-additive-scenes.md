# Multiple Additive Scenes Example

This example demonstrates how to manage multiple additive scenes on the server and synchronize player movement and object interactions within these scenes.

## Setup

1.  In Unity, go to `File > Build Settings`.
2.  Remove all existing scenes from the "Scenes In Build" list.
3.  Add the following scenes from `Assets/Examples/MultipleAdditiveScenes/Scenes` in this order:
    -   `Main.unity`
    -   `Game.unity`

4.  Open the `Main.unity` scene in the Editor.
5.  Select the `NetworkManager` GameObject.
6.  Ensure the `Game Scene` field in the `MultiScene Network Manager` component is set to the `Game.unity` scene. This should be set by default, but verify it.

## Key Components

### `MultiSceneNetManager.cs`

This script extends `NetworkManager` and is central to managing the multiple additive scenes. It handles:

-   **Additive Scene Loading:** On server start, it loads multiple instances of the `Game.unity` scene additively. Each instance has its own physics scene, allowing for isolated physics simulations.
-   **Player Placement:** When a player connects, they are assigned to one of the additive scene instances. The player's `NetworkIdentity` GameObject is moved into the assigned additive scene.
-   **Scene Unloading:** On server or client stop, it unloads the additive scenes to clean up.

### `PhysicsCollision.cs`

Attached to objects within the additive scenes (e.g., the tumblers), this script demonstrates server-authoritative physics. It applies force to rigidbodies when a player collides with them. The `isKinematic` property of the Rigidbody is set to `true` on clients and `false` on the server, ensuring physics simulation only occurs on the server.

### `PhysicsSimulator.cs`

This script is placed on a GameObject within each additive scene. Its purpose is to explicitly simulate the physics for that specific additive scene on the server. Unity's additive scenes with `localPhysicsMode` do not auto-simulate, so this script ensures that physics interactions within each isolated scene are processed.

### `PlayerController.cs`

This script handles player movement and camera control. It demonstrates:

-   **Local Player Control:** Only the local player's input is processed for movement.
-   **Camera Setup:** On `OnStartLocalPlayer`, it configures the main camera to follow the player.
-   **Network Transform:** It relies on the `NetworkTransform` component (attached to the same GameObject) to synchronize the player's position and rotation across the network.

## How to Run

1.  **Run as Host:** Press Play in the Unity Editor. The game will start as a host (server + client).
2.  **Run as Client:** Build and run at least two standalone instances of the game. In each standalone instance, click the "Client" button to connect to the host running in the editor.
3.  Observe how players are distributed across different additive scene instances. You can move around using WASDQE and jump with Space. Collide with the colored spheres to score points and with the tumblers to see server-side physics in action. Notice that scores are only shown for players within the same sub-scene.

This example provides a comprehensive look at managing complex scene structures and physics interactions in a networked environment using Mirage.