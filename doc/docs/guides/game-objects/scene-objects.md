---
sidebar_position: 8
title: Scene Objects
---
# Scene Game Objects

There are two types of networked game objects in Mirage’s multiplayer system:
-   Those that are spawned from prefabs at runtime
-   Those that are saved as part of a Scene

Game objects that are spawned from prefabs at runtime must have their prefabs registered in the ClientObjectManager and are instantiated on client when they are spawned on the server. See [Spawn objects](./spawn-object.md) for more on spawning from prefabs.

Networked game objects that you save as part of a Scene are handled differently, as they already exist in the Scene when it is loaded. After loading a new scene [PrepareToSpawnSceneObjects](/docs/reference/Mirage/ClientObjectManager#preparetospawnsceneobjects) must be called on the client and [SpawnSceneObjects](/docs/reference/Mirage/ServerObjectManager#spawnsceneobjects) on the server. These functions will cause Mirage to find all scene objects and then spawn them for networking. These objects will then have their netId and other network values set once the server has send the SpawnMessage for that object. You may want to disable scene objects to avoid them being in the scene until they are spawned.  

Saving networked game objects in your Scene has some benefits:
- They are loaded with the level
- They can be modify in the scene, rather than requiring multiple prefabs for small changes
- Other game object instances in the Scene can reference them, which can avoid you having to use code to find the game objects and make references to them up at runtime.

Networked scene objects are spawned by the ClientObjectManager and ServerObjectManager and act like any other dynamically spawned game objects. Mirage synchronizes them with updates ClientRPC calls.

If a Scene game object is destroyed on the server before a client joins the game, then it is never spawned on new clients. It will be left in the state it is when you were editing the scene.

After a client has connected and you have called [AddCharacter](/docs/reference/Mirage/ServerObjectManager#addcharacterinetworkplayer-networkidentity) or [SpawnVisibleObjects](/docs/reference/Mirage/ServerObjectManager#spawnvisibleobjectsinetworkplayer-boolean), the client is sent a spawn message for each of the Scene objects that exist on the server, that are visible to that client. This message causes the game object on the client to be enabled and has the latest state of that game object from the server in it. This means that only game objects that are visible to the client and not destroyed on the server, are spawned on the client. Like regular non-Scene objects, these Scene objects are started with the latest state when the client joins the game.


### Filtering Scene Objects

By default, calling `SpawnSceneObjects` or `PrepareToSpawnSceneObjects` will cause Mirage to find all `NetworkIdentity` components in all loaded scenes using `Resources.FindObjectsOfTypeAll<NetworkIdentity>()`.

In some cases, like when running multiple server or client instances in the same unity process, this can be problematic as it might find objects from scenes that don't belong to the current instance.

To solve this, you can provide a custom filter by setting the `SceneObjectFilter` property on the `ServerObjectManager` and `ClientObjectManager`. This allows you to control exactly which `NetworkIdentity` components are included. If the filter is left `null`, the default behavior is used.

**Example: Only include objects from a specific scene**
```csharp
using Mirage;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MySceneManager : MonoBehaviour
{
    public ServerObjectManager serverObjectManager;
    public ClientObjectManager clientObjectManager;
    public Scene myScene;

    // Set the scene to use for filtering
    public void SetScene(Scene scene) 
    {
        myScene = scene;
    }

    void Awake()
    {
        // Set the filter before spawning scene objects
        var filter = (NetworkIdentity identity) =>
        {
            return identity.gameObject.scene == myScene;
        };

        serverObjectManager.SceneObjectFilter = filter;
        clientObjectManager.SceneObjectFilter = filter;

        // Now when SpawnSceneObjects is called, it will only
        // consider objects from `myScene`.
        serverObjectManager.SpawnSceneObjects();
    }
}
```
