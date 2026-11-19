---
id: ServerObjectManagerExtensions
title: ServerObjectManagerExtensions
---

# Class ServerObjectManagerExtensions


Extra helper methods for  that dont add any extra logic



<div class="inheritance">

##### Inheritance

<div class="level" style={{"--data-index": 0}}>
System.Object
</div>
</div>

##### Inherited Members

<details>
<summary>Show</summary>
</details>

##### Syntax

```cs
public static class ServerObjectManagerExtensions
```

### Methods
#### AddCharacter(ServerObjectManager, INetworkPlayer, GameObject, Int32)


When  is received from a player, the server calls this to associate the character GameObject with the NetworkPlayer.
When a character is added for a player the object is automatically spawned, so you do not need to call ServerObjectManager.Spawn for that object.
This function is used for adding a character, not replacing. If there is already a character then use  instead.



##### Declaration

```cs
public static void AddCharacter(this ServerObjectManager som, INetworkPlayer player, GameObject character, int prefabHash)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.ServerObjectManager | som |  |
| Mirage.INetworkPlayer | player | the Player to add the character to |
| GameObject | character | The Network Object to add to the Player. Can be spawned or unspawned. Calling this method will respawn it. |
| System.Int32 | prefabHash | New prefab hash to give to the player, used for dynamically creating objects at runtime. |


#### AddCharacter(ServerObjectManager, INetworkPlayer, GameObject)


When  is received from a player, the server calls this to associate the character GameObject with the NetworkPlayer.
When a character is added for a player the object is automatically spawned, so you do not need to call ServerObjectManager.Spawn for that object.
This function is used for adding a character, not replacing. If there is already a character then use  instead.



##### Declaration

```cs
public static void AddCharacter(this ServerObjectManager som, INetworkPlayer player, GameObject character)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.ServerObjectManager | som |  |
| Mirage.INetworkPlayer | player | the Player to add the character to |
| GameObject | character | The Network Object to add to the Player. Can be spawned or unspawned. Calling this method will respawn it. |


#### ReplaceCharacter(ServerObjectManager, INetworkPlayer, GameObject, Boolean)


This replaces the player object for a connection with a different player object. The old player object is not destroyed.
If a connection already has a player object, this can be used to replace that object with a different player object. This does NOT change the ready state of the connection, so it can safely be used while changing scenes.



##### Declaration

```cs
public static void ReplaceCharacter(this ServerObjectManager som, INetworkPlayer player, GameObject character, bool keepAuthority = false)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.ServerObjectManager | som |  |
| Mirage.INetworkPlayer | player | Connection which is adding the player. |
| GameObject | character | Player object spawned for the player. |
| System.Boolean | keepAuthority | Does the previous player remain attached to this connection? |


#### ReplaceCharacter(ServerObjectManager, INetworkPlayer, GameObject, Int32, Boolean)


This replaces the player object for a connection with a different player object. The old player object is not destroyed.
If a connection already has a player object, this can be used to replace that object with a different player object. This does NOT change the ready state of the connection, so it can safely be used while changing scenes.



##### Declaration

```cs
public static void ReplaceCharacter(this ServerObjectManager som, INetworkPlayer player, GameObject character, int prefabHash, bool keepAuthority = false)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.ServerObjectManager | som |  |
| Mirage.INetworkPlayer | player | Connection which is adding the player. |
| GameObject | character | Player object spawned for the player. |
| System.Int32 | prefabHash |  |
| System.Boolean | keepAuthority | Does the previous player remain attached to this connection? |


#### Spawn(ServerObjectManager, GameObject, GameObject)


Spawns the identity and settings its owner to the player that owns ownerObject



##### Declaration

```cs
public static void Spawn(this ServerObjectManager som, GameObject obj, GameObject ownerObject)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.ServerObjectManager | som |  |
| GameObject | obj |  |
| GameObject | ownerObject | An object owned by a player |


#### Spawn(ServerObjectManager, GameObject, Int32, INetworkPlayer)


Assigns prefabHash to the obj and then spawns it with owner

 can only be set on an identity if the current value is Empty


    This method is useful if you are creating network objects at runtime and both server and client know what  to set on an object




##### Declaration

```cs
public static void Spawn(this ServerObjectManager som, GameObject obj, int prefabHash, INetworkPlayer owner = null)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.ServerObjectManager | som |  |
| GameObject | obj | The object to spawn. |
| System.Int32 | prefabHash | The prefabHash of the object to spawn. Used for custom spawn handlers. |
| Mirage.INetworkPlayer | owner | The connection that has authority over the object |


#### Spawn(ServerObjectManager, GameObject, INetworkPlayer)


Spawns the identity and settings its owner to owner



##### Declaration

```cs
public static void Spawn(this ServerObjectManager som, GameObject obj, INetworkPlayer owner = null)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.ServerObjectManager | som |  |
| GameObject | obj |  |
| Mirage.INetworkPlayer | owner |  |


#### SpawnInstantiate(ServerObjectManager, GameObject, Nullable&lt;Int32&gt;, INetworkPlayer)


Instantiate a prefab an then Spawns it with ServerObjectManager



##### Declaration

```cs
public static GameObject SpawnInstantiate(this ServerObjectManager som, GameObject prefab, int? prefabHash = default(int? ), INetworkPlayer owner = null)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.ServerObjectManager | som |  |
| GameObject | prefab |  |
| System.Nullable&lt;System.Int32&gt; | prefabHash |  |
| Mirage.INetworkPlayer | owner |  |

##### Returns
| Type | Description |
| ---- | ---- |
| GameObject |  |

#### SpawnInstantiate(ServerObjectManager, NetworkIdentity, Nullable&lt;Int32&gt;, INetworkPlayer)


Instantiate a prefab an then Spawns it with ServerObjectManager



##### Declaration

```cs
public static NetworkIdentity SpawnInstantiate(this ServerObjectManager som, NetworkIdentity prefab, int? prefabHash = default(int? ), INetworkPlayer owner = null)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.ServerObjectManager | som |  |
| Mirage.NetworkIdentity | prefab |  |
| System.Nullable&lt;System.Int32&gt; | prefabHash |  |
| Mirage.INetworkPlayer | owner |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.NetworkIdentity |  |

