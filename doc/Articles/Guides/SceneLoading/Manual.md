# Manual scene loading

If you need some special way to load scene it is better to control it manually.

## Using Messages

These message are build in and used by NetworkSceneManager. If you are create your own scene logic then you can re-use these message for your own purpose
- [SceneMessage]() sent to tell client to start a scene load
- [SceneReadyMessage]() sent to either client/server to tell them that they have finished loading


## Loading a scene

The server should mark the player as [not ready](), and send [SceneMessage]().

When the client receives the message they should then load the scene. Once the loading has completed they should call [PrepareToSpawnSceneObjects]() and send [SceneReadyMessage]() back to the server.

[PrepareToSpawnSceneObjects]() Will tell the client to find all scene object. This will allow them to be spawned by message sent from the server.

When the server receives [SceneReadyMessage]() it should mark the player as ready, `player.SceneIsReady = true`. This can then be used on the server to check if all players are ready, and then start the game or match if they are ready.
