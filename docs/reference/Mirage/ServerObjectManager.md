---
id: ServerObjectManager
title: ServerObjectManager
---

# Class ServerObjectManager


The ServerObjectManager.



<div class="inheritance">

##### Inheritance

<div class="level" style={{"--data-index": 0}}>
System.Object
</div>
</div>

##### Syntax

```cs
public class ServerObjectManager : MonoBehaviour
```


### Fields

#### NetIdGenerator

##### Declaration

```cs
public INetIdGenerator NetIdGenerator
```
#### SpawnValuesHandler

##### Declaration

```cs
public ISpawnValuesHandler SpawnValuesHandler
```

### Properties

#### Server

##### Declaration

```cs
public NetworkServer Server { get; }
```
#### DefaultVisibility

##### Declaration

```cs
public INetworkVisibility DefaultVisibility { get; }
```
#### SceneObjectFilter

A filter to only include certain scene objects when spawning.
Used by  when finding scene objects.
If the filter is null, all valid scene objects will be included.


##### Declaration

```cs
public Func<NetworkIdentity, bool> SceneObjectFilter { get; set; }
```
### Methods
#### ReplaceCharacter(INetworkPlayer, NetworkIdentity, Int32, Boolean)


This replaces the player object for a connection with a different player object. The old player object is not destroyed.
If a connection already has a player object, this can be used to replace that object with a different player object. This does NOT change the ready state of the connection, so it can safely be used while changing scenes.



##### Declaration

```cs
public void ReplaceCharacter(INetworkPlayer player, NetworkIdentity character, int prefabHash, bool keepAuthority = false)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.INetworkPlayer | player | Connection which is adding the player. |
| Mirage.NetworkIdentity | character | Player object spawned for the player. |
| System.Int32 | prefabHash |  |
| System.Boolean | keepAuthority | Does the previous player remain attached to this connection? |


#### ReplaceCharacter(INetworkPlayer, NetworkIdentity, Boolean)


This replaces the player object for a connection with a different player object. The old player object is not destroyed.
If a connection already has a player object, this can be used to replace that object with a different player object. This does NOT change the ready state of the connection, so it can safely be used while changing scenes.



##### Declaration

```cs
public void ReplaceCharacter(INetworkPlayer player, NetworkIdentity identity, bool keepAuthority = false)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.INetworkPlayer | player | Connection which is adding the player. |
| Mirage.NetworkIdentity | identity | Player object spawned for the player. |
| System.Boolean | keepAuthority | Does the previous player remain attached to this connection? |


#### AddCharacter(INetworkPlayer, NetworkIdentity, Int32)


When  is received from a player, the server calls this to associate the character GameObject with the NetworkPlayer.
When a character is added for a player the object is automatically spawned, so you do not need to call ServerObjectManager.Spawn for that object.
This function is used for adding a character, not replacing. If there is already a character then use  instead.



##### Declaration

```cs
public void AddCharacter(INetworkPlayer player, NetworkIdentity character, int prefabHash)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.INetworkPlayer | player | the Player to add the character to |
| Mirage.NetworkIdentity | character | The Network Object to add to the Player. Can be spawned or unspawned. Calling this method will respawn it. |
| System.Int32 | prefabHash | New prefab hash to give to the player, used for dynamically creating objects at runtime. |


#### AddCharacter(INetworkPlayer, NetworkIdentity)


When  is received from a player, the server calls this to associate the character GameObject with the NetworkPlayer.
When a character is added for a player the object is automatically spawned, so you do not need to call ServerObjectManager.Spawn for that object.
This function is used for adding a character, not replacing. If there is already a character then use  instead.



##### Declaration

```cs
public void AddCharacter(INetworkPlayer player, NetworkIdentity identity)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.INetworkPlayer | player | the Player to add the character to |
| Mirage.NetworkIdentity | identity |  |


#### RemoveCharacter(INetworkPlayer, Boolean)


Removes the character from a player, with the option to keep the player as the owner of the object



##### Declaration

```cs
public void RemoveCharacter(INetworkPlayer player, bool keepAuthority = false)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.INetworkPlayer | player |  |
| System.Boolean | keepAuthority |  |


#### DestroyCharacter(INetworkPlayer, Boolean)


Removes and destroys the character from a player



##### Declaration

```cs
public void DestroyCharacter(INetworkPlayer player, bool destroyServerObject = true)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.INetworkPlayer | player |  |
| System.Boolean | destroyServerObject |  |


#### Spawn(NetworkIdentity, Int32, INetworkPlayer)


Assigns prefabHash to the identity and then spawns it with owner

 can only be set to a non-zero value.


 will be cleared when calling this method, this will ensure that the object is spawned using the new PrefabHash rather than SceneId


    This method is useful if you are creating network objects at runtime and both server and client know what  to set on an object




##### Declaration

```cs
public void Spawn(NetworkIdentity identity, int prefabHash, INetworkPlayer owner = null)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.NetworkIdentity | identity |  |
| System.Int32 | prefabHash | The prefabHash of the object to spawn. Used for custom spawn handlers. |
| Mirage.INetworkPlayer | owner | The connection that has authority over the object |


#### Spawn(NetworkIdentity, INetworkPlayer)


Spawns the identity and keeping owner as 



##### Declaration

```cs
public void Spawn(NetworkIdentity identity, INetworkPlayer owner)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.NetworkIdentity | identity |  |
| Mirage.INetworkPlayer | owner |  |


#### Spawn(NetworkIdentity)


Spawns the identity and assigns owner to be it&apos;s owner



##### Declaration

```cs
public void Spawn(NetworkIdentity identity)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.NetworkIdentity | identity |  |


#### Destroy(GameObject, Boolean)


Destroys this object and corresponding objects on all clients.
Game object to destroy.
Sets if server object will also be destroyed



##### Declaration

```cs
public void Destroy(GameObject gameObject, bool destroyServerObject = true)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| GameObject | gameObject |  |
| System.Boolean | destroyServerObject |  |


#### Destroy(NetworkIdentity, Boolean)


Destroys this object and corresponding objects on all clients.
Game object to destroy.
Sets if server object will also be destroyed



##### Declaration

```cs
public void Destroy(NetworkIdentity identity, bool destroyServerObject = true)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.NetworkIdentity | identity |  |
| System.Boolean | destroyServerObject |  |


#### SpawnSceneObjects()


This causes NetworkIdentity objects in a scene to be spawned on a server.

    Calling SpawnObjects() causes all scene objects to be spawned.
    It is like calling Spawn() for each of them.




##### Declaration

```cs
public void SpawnSceneObjects()
```


#### SpawnVisibleObjects(INetworkPlayer)


Sends spawn message for scene objects and other visible objects to the given player if it has a character

If there is a scene helper (e.g. ) then this will be called after the client finishes loading the scene and sends 




##### Declaration

```cs
public void SpawnVisibleObjects(INetworkPlayer player)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.INetworkPlayer | player | The player to spawn objects for |


#### SpawnVisibleObjects(INetworkPlayer, Boolean)


Sends spawn message for scene objects and other visible objects to the given player if it has a character



##### Declaration

```cs
public void SpawnVisibleObjects(INetworkPlayer player, bool ignoreHasCharacter)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.INetworkPlayer | player | The player to spawn objects for |
| System.Boolean | ignoreHasCharacter | If true will spawn visible objects even if player does not have a spawned character yet |


#### SpawnVisibleObjects(INetworkPlayer, NetworkIdentity)


Sends spawn message for scene objects and other visible objects to the given player if it has a character



##### Declaration

```cs
public void SpawnVisibleObjects(INetworkPlayer player, NetworkIdentity skip)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.INetworkPlayer | player | The player to spawn objects for |
| Mirage.NetworkIdentity | skip |  |


#### SpawnVisibleObjects(INetworkPlayer, Boolean, NetworkIdentity)


Sends spawn message for scene objects and other visible objects to the given player if it has a character



##### Declaration

```cs
public void SpawnVisibleObjects(INetworkPlayer player, bool ignoreHasCharacter, NetworkIdentity skip)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.INetworkPlayer | player | The player to spawn objects for |
| System.Boolean | ignoreHasCharacter | If true will spawn visible objects even if player does not have a spawned character yet |
| Mirage.NetworkIdentity | skip | NetworkIdentity to skip when spawning. Can be null |


#### SpawnVisibleObjects(INetworkPlayer, Boolean, HashSet&lt;NetworkIdentity&gt;)


Sends spawn message for scene objects and other visible objects to the given player if it has a character



##### Declaration

```cs
public void SpawnVisibleObjects(INetworkPlayer player, bool ignoreHasCharacter, HashSet<NetworkIdentity> skip)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.INetworkPlayer | player | The player to spawn objects for |
| System.Boolean | ignoreHasCharacter | If true will spawn visible objects even if player does not have a spawned character yet |
| System.Collections.Generic.HashSet&lt;Mirage.NetworkIdentity&gt; | skip | NetworkIdentity to skip when spawning. Can be null |


