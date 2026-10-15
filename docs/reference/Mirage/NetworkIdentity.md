---
id: NetworkIdentity
title: NetworkIdentity
---

# Class NetworkIdentity


The NetworkIdentity identifies objects across the network, between server and clients.
Its primary data is a NetworkInstanceId which is allocated by the server and then set on clients.
This is used in network communications to be able to lookup game objects on different machines.



<div class="inheritance">

##### Inheritance

<div class="level" style={{"--data-index": 0}}>
System.Object
</div>
</div>

##### Syntax

```cs
public sealed class NetworkIdentity : MonoBehaviour
```


### Fields

#### SpawnSettings

##### Declaration

```cs
public NetworkSpawnSettings SpawnSettings
```
#### AuthorityRemovedTime

Time on server when authority was removed, and who it was removed from. Can be used to ignore RPCs that were sent before client knew about authority change
Value is 0 if authority has not been removed. Only valid on server


##### Declaration

```cs
public AuthorityTime AuthorityRemovedTime
```
#### observers

The set of network connections (players) that can see this object.


##### Declaration

```cs
public readonly HashSet<INetworkPlayer> observers
```
#### ServerObjectManager

The ServerObjectManager is present only for server/host instances.


##### Declaration

```cs
public ServerObjectManager ServerObjectManager
```
#### ClientObjectManager

The ClientObjectManager is present only for client instances.


##### Declaration

```cs
public ClientObjectManager ClientObjectManager
```

### Properties

#### IsClient

Returns true if running as a client and this object was spawned by a server.


##### Declaration

```cs
public bool IsClient { get; }
```
#### IsServer

Returns true if NetworkServer.active and server is not stopped.


##### Declaration

```cs
public bool IsServer { get; }
```
#### IsLocalClient

Returns true if we&apos;re on host mode.


##### Declaration

```cs
[Obsolete("use IsHost instead")]
public bool IsLocalClient { get; }
```
#### IsHost

Returns true if we&apos;re on host mode.


##### Declaration

```cs
public bool IsHost { get; }
```
#### IsLocalPlayer

This returns true if this object is the one that represents the player on the local machine.
This is set when the server has spawned an object for this particular client.


##### Declaration

```cs
public bool IsLocalPlayer { get; }
```
#### HasAuthority

This returns true if this object is the authoritative player object on the client.
This value is determined at runtime. For most objects, authority is held by the server.
For objects that had their authority set by AssignClientAuthority on the server, this will be true on the client that owns the object. NOT on other clients.


##### Declaration

```cs
public bool HasAuthority { get; }
```
#### NetId

Unique identifier for this particular object instance, used for tracking objects between networked clients and the server.
This is a unique identifier for this particular GameObject instance. Use it to track GameObjects between networked clients and the server.


##### Declaration

```cs
public uint NetId { get; }
```
#### SceneId

##### Declaration

```cs
public ulong SceneId { get; }
```
#### IsSceneObject

Is this object part of a scene and have a Scene Id?


##### Declaration

```cs
public bool IsSceneObject { get; }
```
#### IsPrefab

Is this object a prefab and have a  so that it can be spawned over the network


##### Declaration

```cs
public bool IsPrefab { get; }
```
#### IsSpawned

Has this object been spawned and have a 


##### Declaration

```cs
public bool IsSpawned { get; }
```
#### Server

The NetworkServer associated with this NetworkIdentity.


##### Declaration

```cs
public NetworkServer Server { get; }
```
#### World

The world this object exists in


##### Declaration

```cs
public NetworkWorld World { get; }
```
#### SyncVarSender

##### Declaration

```cs
public SyncVarSender SyncVarSender { get; }
```
#### InitialState

True while applying spawn payload within OnDeserializeAll
Can be used inside syncvar hooks to tell if object has just spawned


##### Declaration

```cs
public bool InitialState { get; }
```
#### Client

The NetworkClient associated with this NetworkIdentity.


##### Declaration

```cs
public NetworkClient Client { get; }
```
#### Owner

The INetworkPlayer associated with this . This property is only valid on server
Use it to return details such as the connection&apos;s identity, IP address and ready status.


##### Declaration

```cs
public INetworkPlayer Owner { get; }
```
#### NetworkBehaviours

Array of NetworkBehaviours associated with this NetworkIdentity. Can be in child GameObjects.


##### Declaration

```cs
public NetworkBehaviour[] NetworkBehaviours { get; }
```
#### Visibility

Returns the NetworkVisibility behaviour on this gameObject, or a default visibility where all objects are visible.
Note: NetworkVisibility must be on same gameObject has NetworkIdentity, not on a child object


##### Declaration

```cs
public INetworkVisibility Visibility { get; }
```
#### PrefabHash

##### Declaration

```cs
public int PrefabHash { get; set; }
```
#### OnStartServer

This is invoked for NetworkBehaviour objects when they become active on the server.
This could be triggered by NetworkServer.Start() for objects in the scene, or by ServerObjectManager.Spawn() for objects you spawn at runtime.
This will be called for objects on a &quot;host&quot; as well as for object on a dedicated server.
OnStartServer is invoked before this object is added to collection of spawned objects


##### Declaration

```cs
public IAddLateEvent OnStartServer { get; }
```
#### OnStartClient

Called on every NetworkBehaviour when it is activated on a client.
Objects on the host have this function called, as there is a local client on the host. The values of SyncVars on object are guaranteed to be initialized
correctly with the latest state from the server when this function is called on the client.


##### Declaration

```cs
public IAddLateEvent OnStartClient { get; }
```
#### OnStartLocalPlayer

Called when the local player object has been set up.
This happens after OnStartClient(), as it is triggered by an ownership message from the server. This is an appropriate place to activate components or
functionality that should only be active for the local player, such as cameras and input.


##### Declaration

```cs
public IAddLateEvent OnStartLocalPlayer { get; }
```
#### OnAuthorityChanged

This is invoked on behaviours that have authority given or removed, see 
This is called after  and before 

When  or  is called on the server, this will be called on the client that owns the object.


When an object is spawned with  with a NetworkConnection parameter included,
this will be called on the client that owns the object.

NOTE: this even is only called for client and host


##### Declaration

```cs
public IAddLateEvent<bool> OnAuthorityChanged { get; }
```
#### OnOwnerChanged

This is invoked on behaviours that have an owner assigned.
This even is only called on server
See  for more comments on owner and authority


##### Declaration

```cs
public IAddLateEvent<INetworkPlayer> OnOwnerChanged { get; }
```
#### OnStopClient

This is invoked on clients when the server has caused this object to be destroyed.
This can be used as a hook to invoke effects or do client specific cleanup.


##### Declaration

```cs
public IAddLateEvent OnStopClient { get; }
```
#### OnStopServer

This is called on the server when the object is unspawned


##### Declaration

```cs
public IAddLateEvent OnStopServer { get; }
```
#### SpawnedFromInstantiate

##### Declaration

```cs
public bool SpawnedFromInstantiate { get; }
```
#### RemoteCallCollection

##### Declaration

```cs
public RemoteCallCollection RemoteCallCollection { get; }
```
### Methods
#### ClearSceneId()


Used to clear . Use in cases where you want to spawn object in custom way using Spawn Handlers with AssetId.



##### Declaration

```cs
public void ClearSceneId()
```


#### RebuildObservers(Boolean)


This causes the set of players that can see this object to be rebuild.
The OnRebuildObservers callback function will be invoked on each NetworkBehaviour.



##### Declaration

```cs
public void RebuildObservers(bool initialize)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Boolean | initialize | True if this is the first time. |


#### AssignClientAuthority(INetworkPlayer)


Assign control of an object to a client via the client&apos;s 
This causes hasAuthority to be set on the client that owns the object, and NetworkBehaviour.OnStartAuthority will be called on that client. This object then will be
in the NetworkConnection.clientOwnedObjects list for the connection.
Authority can be removed with RemoveClientAuthority. Only one client can own an object at any time. This does not need to be called for player objects, as their
authority is setup automatically.



##### Declaration

```cs
public void AssignClientAuthority(INetworkPlayer player)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.INetworkPlayer | player | The connection of the client to assign authority to. |


#### RemoveClientAuthority()


Removes ownership for an object.
This applies to objects that had authority set by AssignClientAuthority, or  with a NetworkConnection
parameter included.
Authority cannot be removed for player objects.



##### Declaration

```cs
public void RemoveClientAuthority()
```


#### ClearNetworkBehaviourCache()


Can be used to clear the Cache for .
Use this if you need to change what behaviours are part of this identity.
WARNING: changing behaviour at runtime could lead to desync, use at your own risk



##### Declaration

```cs
public void ClearNetworkBehaviourCache()
```


#### ToString()



##### Declaration

```cs
public override string ToString()
```

##### Returns
| Type | Description |
| ---- | ---- |
| System.String |  |

