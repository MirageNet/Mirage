---
id: NetworkSceneChecker
title: NetworkSceneChecker
---

# Class NetworkSceneChecker


Component that controls visibility of networked objects between scenes.
Any object with this component on it will only be visible to other objects in the same scene
This would be used when the server has multiple additive subscenes loaded to isolate players to their respective subscenes



<div class="inheritance">

##### Inheritance

<div class="level" style={{"--data-index": 0}}>
System.Object
</div>
<div class="level" style={{"--data-index": 1}}>
Mirage.NetworkBehaviour
</div>
<div class="level" style={{"--data-index": 2}}>
Mirage.NetworkVisibility
</div>
</div>

##### Inherited Members

<details>
<summary>Show</summary>

Mirage.NetworkVisibility.OnVisibilityChanged


Mirage.NetworkBehaviour._nextSyncTime


Mirage.NetworkBehaviour.SyncSettings


Mirage.NetworkBehaviour.IsServer


Mirage.NetworkBehaviour.IsClient


Mirage.NetworkBehaviour.IsLocalClient


Mirage.NetworkBehaviour.IsHost


Mirage.NetworkBehaviour.IsLocalPlayer


Mirage.NetworkBehaviour.IsServerOnly


Mirage.NetworkBehaviour.IsClientOnly


Mirage.NetworkBehaviour.HasAuthority


Mirage.NetworkBehaviour.NetId


Mirage.NetworkBehaviour.Server


Mirage.NetworkBehaviour.ServerObjectManager


Mirage.NetworkBehaviour.Client


Mirage.NetworkBehaviour.ClientObjectManager


Mirage.NetworkBehaviour.Owner


Mirage.NetworkBehaviour.World


Mirage.NetworkBehaviour.NetworkTime


Mirage.NetworkBehaviour.BehaviourId


Mirage.NetworkBehaviour.SyncVarDirtyBits


Mirage.NetworkBehaviour.AnySyncObjectDirty


Mirage.NetworkBehaviour.syncObjects


Mirage.NetworkBehaviour.Identity


Mirage.NetworkBehaviour.COMPONENT_INDEX_NOT_FOUND


Mirage.NetworkBehaviour.ComponentIndex


Mirage.NetworkBehaviour.InitSyncObject(Mirage.Collections.ISyncObject)


Mirage.NetworkBehaviour.UpdateSyncObjectShouldSync()


Mirage.NetworkBehaviour.SyncVarEqual&lt;T&gt;(T, T)


Mirage.NetworkBehaviour.ClearDirtyBits()


Mirage.NetworkBehaviour.AnyDirtyBits()


Mirage.NetworkBehaviour.SerializeObjectsAll(Mirage.Serialization.NetworkWriter)


Mirage.NetworkBehaviour.SerializeObjectsDelta(Mirage.Serialization.NetworkWriter)


Mirage.NetworkBehaviour.GetRpcCount()


Mirage.NetworkBehaviour.RegisterRpc(Mirage.RemoteCalls.RemoteCallCollection)

</details>

##### Syntax

```cs
[Obsolete("This checker is inefficient, use SceneVisibilityChecker instead")]
public class NetworkSceneChecker : NetworkVisibility, INetworkVisibility
```


### Fields

#### forceHidden

Flag to force this object to be hidden from all observers.
If this object is a player object, it will not be hidden for that client.


##### Declaration

```cs
public bool forceHidden
```
### Methods
#### OnStartServer()



##### Declaration

```cs
public void OnStartServer()
```


#### OnCheckObserver(INetworkPlayer)


Callback used by the visibility system to determine if an observer (player) can see this object.
If this function returns true, the network connection will be added as an observer.



##### Declaration

```cs
public override bool OnCheckObserver(INetworkPlayer player)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.INetworkPlayer | player | Network connection of a player. |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean | True if the player can see this object. |

#### OnRebuildObservers(HashSet&lt;INetworkPlayer&gt;, Boolean)


Callback used by the visibility system to (re)construct the set of observers that can see this object.
Implementations of this callback should add network connections of players that can see this object to the observers set.



##### Declaration

```cs
public override void OnRebuildObservers(HashSet<INetworkPlayer> observers, bool initialize)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Collections.Generic.HashSet&lt;Mirage.INetworkPlayer&gt; | observers | The new set of observers for this object. |
| System.Boolean | initialize | True if the set of observers is being built for the first time. |


