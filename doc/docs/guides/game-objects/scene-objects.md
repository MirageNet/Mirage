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
