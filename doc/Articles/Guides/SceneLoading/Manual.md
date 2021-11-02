# Manual scene loading

If [NetworkSceneManager](./NetworkSceneManager.md) doesn't work for your case you can control everything yourself.


## Using Messages

These messages are built in and used by NetworkSceneManager. If you are creating your own scene logic then you can re-use these messages for your own purpose.
- <xref:Mirage.SceneMessage> Sent to client to load a scene
- <xref:Mirage.SceneReadyMessage> Sent to either client or server when they have finished loading


## Loading a scene

**Server**
1) Mark `Player` as not ready, using <xref:Mirage.NetworkPlayer.SceneIsReady>
2) Send `SceneMessage` to clients

**Client** 

*after receiving `SceneMessage`*

3) (optional) Mark local player as not ready 
4) Load the scene

*after loading finished*

5) Call [ClientObjectManager.PrepareToSpawnSceneObjects](xref:Mirage.ClientObjectManager.PrepareToSpawnSceneObjects) (This will tell Mirage about any new scene objects)
6) (optional) Mark local player as ready 
7) Send `SceneReadyMessage` to server

**Server** 

*after receiving `SceneReadyMessage`*

8) Mark the player as ready using: `player.SceneIsReady = true`
9) Call [ServerObjectManager.SpawnVisibleObjects](xref:Mirage.ServerObjectManager.SpawnVisibleObjects(Mirage.INetworkPlayer,System.Boolean)) or [ServerObjectManager.AddCharacter](xref:Mirage.ServerObjectManager.AddCharacter(Mirage.INetworkPlayer,Mirage.NetworkIdentity)) (Mirage will send spawn message to client)


