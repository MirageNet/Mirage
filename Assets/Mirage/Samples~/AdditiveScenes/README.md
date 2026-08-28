# Additive Scenes Example

The Additive Scenes example demonstrates spatial sub-scene loading and unloading based on trigger zones, network entity animation via RPCs, and interest management using `SubSceneProximityChecker`.

The server loads `SubScene` when it starts. Each client only loads `SubScene` while its player is inside the zone, and `SubSceneLoader` only shows the objects in it once the client has finished loading.

> [!IMPORTANT]
> Make sure your Unity project has a layer named `Player` configured in Project Settings -> Tags and Layers for trigger detection to work properly.

## Build Settings Setup

In Build Settings, remove all scenes and add the scenes from the `Scenes` folder in the following order:
- `MainScene`
- `SubScene`

## How to Play

1. Open `MainScene.unity` in the Editor.
2. Press **Play** and click **Host (Server + Client)** in the HUD.
3. Start up to 3 built client instances and click **Client**.
4. Use **WASDQE** to move and turn your player capsule.
5. Move towards the central shelter (trigger zone):
   - Entering the zone commands the client to load `SubScene` additively, and the networked shapes inside it appear.
   - Leaving the shelter area hides the shapes and unloads the sub-scene.
