# Multiple Additive Scenes Example

Runs several isolated game instances on a single server. Each instance is an additive copy of the `Game` scene with its own physics scene.

## Build Settings Setup

In Build Settings, remove all scenes and add the scenes from the `Scenes` folder in the following order:
- `Main`
- `Game`

## MultiScene Network Manager

`MultiSceneNetManager` is derived from `NetworkManager`. Its scene loading is based on `NetworkSceneLoader`:
- The server loads `Instances` copies of the Game scene with `localPhysicsMode = LocalPhysicsMode.Physics3D`.
- Each client loads a single copy of the Game scene (without `localPhysicsMode`), then tells the server it is ready.
- The server then spawns the player's character, moves it into one of the instances, and sets its `PlayerScore` SyncVars.

Physics scenes do not auto-simulate, so each Game scene has a `PhysicsSimulator` that runs physics on the server only.

All networked objects have a `NetworkSceneChecker` component, which isolates them to their own instance.

## How to Play

1. Open `Main.unity`, press **Play** and click **Host (Server + Client)** in the HUD. You can also use **Server Only**.
2. Start at least 3 built instances and click **Client**.
3. Use **WASDQE** to move and turn your player capsule, and **Space** to jump.
   - Colliding with the small colored spheres scores points based on their color.
   - Colliding with the larger tumblers sends them rolling around. They are server-side non-kinematic rigidbodies.
   - Only scores for players in the same instance are shown at the top of the game window.
