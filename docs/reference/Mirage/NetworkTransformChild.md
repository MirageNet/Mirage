---
id: NetworkTransformChild
title: NetworkTransformChild
---

# Class NetworkTransformChild


A component to synchronize the position of child transforms of networked objects.
There must be a NetworkTransform on the root object of the hierarchy. There can be multiple NetworkTransformChild components on an object. This does not use physics for synchronization, it simply synchronizes the localPosition and localRotation of the child transform and lerps towards the received values.



<div class="inheritance">

##### Inheritance

<div class="level" style={{"--data-index": 0}}>
System.Object
</div>
<div class="level" style={{"--data-index": 1}}>
Mirage.NetworkBehaviour
</div>
<div class="level" style={{"--data-index": 2}}>
Mirage.NetworkTransformBase
</div>
</div>

##### Inherited Members

<details>
<summary>Show</summary>

Mirage.NetworkTransformBase.ClientAuthority


Mirage.NetworkTransformBase.LocalPositionSensitivity


Mirage.NetworkTransformBase.LocalRotationSensitivity


Mirage.NetworkTransformBase.LocalScaleSensitivity


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
public class NetworkTransformChild : NetworkTransformBase
```


### Fields

#### Target

##### Declaration

```cs
public Transform Target
```

### Properties

#### TargetComponent

##### Declaration

```cs
protected override Transform TargetComponent { get; }
```
