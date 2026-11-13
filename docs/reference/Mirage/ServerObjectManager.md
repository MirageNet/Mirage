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
public NetworkIdentityFilter SceneObjectFilter { get; set; }
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


This causes NetworkIdentity objects in active scenes to be spawned on a server.

This will call  on each NetworkIdentity. Objects that are already spawned will have  called with initialize true, causing a Spawn message to be sent again to every ready player, including players that already have the object.


Objects that are already spawned are processed first, sorted by netid. Objects that are not yet spawned come after them, sorted by sceneId.
Because sceneId starts with a hash of the scene path, if you have multiple active scenes then NetworkIdentity in the same scene will spawn together.


This version of SpawnSceneObjects uses the  field to filter which objects are spawned, by default this filter is null.
You can call  with a custom filter if you want to decide what objects get spawned for a single SpawnSceneObjects call.




##### Declaration

```cs
public void SpawnSceneObjects()
```


#### SpawnSceneObjects(NetworkIdentityFilter)


This causes NetworkIdentity objects in active scenes to be spawned on a server.

This will call  on each NetworkIdentity. Objects that are already spawned will have  called with initialize true, causing a Spawn message to be sent again to every ready player, including players that already have the object.


Objects that are already spawned are processed first, sorted by netid. Objects that are not yet spawned come after them, sorted by sceneId.
Because sceneId starts with a hash of the scene path, if you have multiple active scenes then NetworkIdentity in the same scene will spawn together.


This version of SpawnSceneObjects ignores the global  filter (can be null) and only uses the custom filter argument instead.
If you need SceneObjectFilter as well your filter method should call it and return the value.

An example use for a custom filter might be to spawn all objects from Scene A before Scene B.
To do this you would call SpawnSceneObjects with a filter for just sceneA, then call it a 2nd time with a filter for just sceneB.
For this type of use case you would not need a matching filter for  as all objects will still be spawned




##### Declaration

```cs
public void SpawnSceneObjects(NetworkIdentityFilter filter)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.NetworkIdentityFilter | filter |  |


#### SpawnVisibleObjects(INetworkPlayer)


Checks all objects spawned on the server to see if they should be visible to player and sends a  if they should be.

 calls this method, so you only need to call it your self if you have custom scene loading that does not call AddCharacter after scene load.




##### Declaration

```cs
public void SpawnVisibleObjects(INetworkPlayer player)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.INetworkPlayer | player | The player to spawn objects for |


#### SpawnVisibleObjects(INetworkPlayer, Boolean)


Checks all objects spawned on the server to see if they should be visible to player and sends a  if they should be.

 calls this method, so you only need to call it your self if you have custom scene loading that does not call AddCharacter after scene load.




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


Checks all objects spawned on the server to see if they should be visible to player and sends a  if they should be.

 calls this method, so you only need to call it your self if you have custom scene loading that does not call AddCharacter after scene load.




##### Declaration

```cs
public void SpawnVisibleObjects(INetworkPlayer player, NetworkIdentity skip)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.INetworkPlayer | player | The player to spawn objects for |
| Mirage.NetworkIdentity | skip | Can be used to skip 1 object, like if you need to spawn all other objects and then that 1 object after all the rest are spawned. |


#### SpawnVisibleObjects(INetworkPlayer, Boolean, NetworkIdentity)


Checks all objects spawned on the server to see if they should be visible to player and sends a  if they should be.

 calls this method, so you only need to call it your self if you have custom scene loading that does not call AddCharacter after scene load.




##### Declaration

```cs
public void SpawnVisibleObjects(INetworkPlayer player, bool ignoreHasCharacter, NetworkIdentity skip)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.INetworkPlayer | player | The player to spawn objects for |
| System.Boolean | ignoreHasCharacter | If true will spawn visible objects even if player does not have a spawned character yet |
| Mirage.NetworkIdentity | skip | Can be used to skip 1 object, like if you need to spawn all other objects and then that 1 object after all the rest are spawned. |


#### SpawnVisibleObjects(INetworkPlayer, Boolean, HashSet&lt;NetworkIdentity&gt;)


Checks all objects spawned on the server to see if they should be visible to player and sends a  if they should be.

 calls this method, so you only need to call it your self if you have custom scene loading that does not call AddCharacter after scene load.




##### Declaration

```cs
public void SpawnVisibleObjects(INetworkPlayer player, bool ignoreHasCharacter, HashSet<NetworkIdentity> skip)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.INetworkPlayer | player | The player to spawn objects for |
| System.Boolean | ignoreHasCharacter | If true will spawn visible objects even if player does not have a spawned character yet |
| System.Collections.Generic.HashSet&lt;Mirage.NetworkIdentity&gt; | skip | Optional argument. Can be used to skip many objects, like if you need to spawn all other objects and then the set of other object after all the rest are spawned. |


