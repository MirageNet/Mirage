---
id: SceneVisibilityChecker
title: SceneVisibilityChecker
---

# Class SceneVisibilityChecker



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
public class SceneVisibilityChecker : NetworkVisibility, INetworkVisibility
```

### Methods
#### OnCheckObserver(INetworkPlayer)



##### Declaration

```cs
public override bool OnCheckObserver(INetworkPlayer player)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.INetworkPlayer | player |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean |  |

#### OnRebuildObservers(HashSet&lt;INetworkPlayer&gt;, Boolean)



##### Declaration

```cs
public override void OnRebuildObservers(HashSet<INetworkPlayer> observers, bool initialize)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Collections.Generic.HashSet&lt;Mirage.INetworkPlayer&gt; | observers |  |
| System.Boolean | initialize |  |


#### MoveToScene(Scene)


Call this function on an object to move it to a new scene and rebuild its observers



##### Declaration

```cs
public void MoveToScene(Scene scene)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Scene | scene |  |


