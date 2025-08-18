# Scene Visibility Checkers

:::info
This document covers two components for managing object visibility based on scenes: `NetworkSceneChecker` and `SceneVisibilityChecker`.
:::

## Overview

Both `NetworkSceneChecker` and `SceneVisibilityChecker` control the visibility of game objects for network clients based on which scene they're in. This is particularly useful when the server has multiple additive sub-scenes loaded and needs to isolate networked objects to their respective sub-scenes.

By using these checkers, a game running on a client doesn’t receive information about game objects that are not visible. This has two main benefits: it reduces the amount of data sent across the network, and it makes your game more secure against hacking.

A game object with either of these components must also have a `NetworkIdentity` component. When you add one of these components to a game object, Mirage also adds a `NetworkIdentity` component on that game object if it does not already have one.

## SceneVisibilityChecker (Recommended)

`SceneVisibilityChecker` is the recommended component for managing object visibility based on scenes. Unlike `NetworkSceneChecker`, it does not perform per-frame checks, making it more efficient.

To use `SceneVisibilityChecker`, you should explicitly call its `MoveToScene(scene)` method when an object changes scenes. This method handles updating observer lists efficiently.

## NetworkSceneChecker (Avoid)

:::danger
`NetworkSceneChecker` is inefficient and should generally be avoided. It checks the scene of objects every frame, which can lead to performance issues.
:::

The `NetworkSceneChecker` component controls the visibility of game objects for network clients, based on which scene they're in.

![Network Scene Checker component](/img/components/NetworkSceneChecker.png)

-   **Force Hidden**
    Tick this checkbox to hide this object from all players.

Scene objects with a `NetworkSceneChecker` component are disabled when they're not in the same scene, and spawned objects are destroyed when they're not in the same scene.


### How to Use `SceneVisibilityChecker` with Additive Scenes

In Mirage, the Server and connected Clients are always on the same main scene, however, the server and clients can have various combinations of smaller sub-scenes loaded additively. The server may load all sub-scenes at start, or it may dynamically load and unload sub-scenes where players or other activity is going on as needed.

All character objects are always first spawned in the main scene, which may or may not have visual content, networked objects, etc. With `SceneVisibilityChecker` attached to all networked objects, whenever the character object is moved to a sub-scene (from the main or another sub-scene) using `MoveToScene`, the observer's lists for objects in both the new scene and the prior scene are updated accordingly.

**Loading the sub-scene(s) on the server:**

```cs
SceneManager.LoadSceneAsync(subScene, LoadSceneMode.Additive);
```

**Sending a `SceneMessage` to the client to load a sub-scene additively:**

```cs
SceneMessage msg = new SceneMessage
{
    sceneName = subScene,
    sceneOperation = SceneOperation.LoadAdditive
};

Owner.Send(msg);
```

**Moving the character object to the sub-scene using `SceneVisibilityChecker.MoveToScene`:**

```cs
// Get the SceneVisibilityChecker component
SceneVisibilityChecker sceneChecker = player.GetComponent<SceneVisibilityChecker>();
if (sceneChecker != null)
{
    // Position the character object in world space first (if needed)
    // This assumes it has a NetworkTransform component that will update clients
    player.transform.position = new Vector3(100, 1, 100);

    // Then move the character object to the subscene using the checker's method
    sceneChecker.MoveToScene(subScene);
}
```

Optionally you can send another `SceneMessage` to the client with `SceneOperation.UnloadAdditive` to remove any previous additive scene the client no longer needs. This would apply to a game that has levels after a level change. A short delay may be necessary before removal to allow the client to get fully synced.

Depending on the complexity of your game, you may find it helpful when switching a player between sub-scenes to move the character object to the main scene first, yield 100 ms, re-position it, and finally move it to the new sub-scene.