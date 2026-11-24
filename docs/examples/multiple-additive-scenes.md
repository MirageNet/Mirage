# Multiple Additive Scenes Example

In Build Settings, remove all scenes and add both of the scenes from the Scenes folder in the following order:

- Main
- Game

Open the Main scene in the Editor and make sure the Game Scene field in the MultiScene Network Manager on the Network scene object contains the Game scene. This is already set up by default, but if the Main scene was opened and saved before putting the scenes in the Build Settings list, the Game Scene field may be cleared accidentally.

## MultiScene Network Manager

The MultiScene Network Manager is derived from the base Network Manager and is responsible for additively loading the sub-scene instances and placing the players in their respective sub-scene instances and initializing player SyncVars. It has a Game Scene field where the Game sub-scene is assigned, and an Instances field to set how many instances are loaded on the server.

In this example, the sub-scene instances are additively loaded on the server with `localPhysicsMode = LocalPhysicsMode.Physics3D`. Physics sub-scenes do not auto-simulate, so each scene has a game object with a generic `PhysicsSimulator` script on it. This script does nothing on the client, only on the server.

Clients only ever have one instance of the sub-scene additively loaded (without `localPhysicsMode`), while the server has them all. All networked objects have a `NetworkSceneChecker` component which is what isolates them to their specific sub-scene.

## Playing in the Instances

File -\> Build and Run

Start at least 3 built instances: These will all be client players.

Press Play in the Editor and click Host (Server + Client) in the HUD - This will be the host and the 1st player. You can also use Server Only if you prefer.

Click Client in the built instances.

-   WASDQE keys to move & turn your player capsule, Space to jump.

-   Colliding with the small colored spheres scores points based on their color.

-   Colliding with the larger tumblers sends them rolling around...they're server-side non-kinematic rigidbodies.

-   Only scores for the players in the same sub-scene are shown at the top of the game window.

![MultiScene Network Manager](/img/examples/multiple-additive-scenes/MultiSceneNetworkManager.PNG)
