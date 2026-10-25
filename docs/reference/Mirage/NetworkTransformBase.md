---
id: NetworkTransformBase
title: NetworkTransformBase
---

# Class NetworkTransformBase



<div class="inheritance">

##### Inheritance

<div class="level" style={{"--data-index": 0}}>
System.Object
</div>
<div class="level" style={{"--data-index": 1}}>
Mirage.NetworkBehaviour
</div>
</div>

##### Inherited Members

<details>
<summary>Show</summary>

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
[Obsolete("NetworkTransform is not optimzied so should not used in production. Use NetworkPositionSync instead")]
public abstract class NetworkTransformBase : NetworkBehaviour
```


### Fields

#### ClientAuthority

##### Declaration

```cs
public bool ClientAuthority
```
#### LocalPositionSensitivity

##### Declaration

```cs
public float LocalPositionSensitivity
```
#### LocalRotationSensitivity

##### Declaration

```cs
public float LocalRotationSensitivity
```
#### LocalScaleSensitivity

##### Declaration

```cs
public float LocalScaleSensitivity
```

### Properties

#### TargetComponent

##### Declaration

```cs
protected abstract Transform TargetComponent { get; }
```
### Methods
#### SerializeIntoWriter(NetworkWriter, Vector3, Quaternion, Vector3)



##### Declaration

```cs
public static void SerializeIntoWriter(NetworkWriter writer, Vector3 position, Quaternion rotation, Vector3 scale)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| Vector3 | position |  |
| Quaternion | rotation |  |
| Vector3 | scale |  |


#### OnSerialize(NetworkWriter, Boolean)



##### Declaration

```cs
public override bool OnSerialize(NetworkWriter writer, bool initialState)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| System.Boolean | initialState |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean |  |

#### OnDeserialize(NetworkReader, Boolean)



##### Declaration

```cs
public override void OnDeserialize(NetworkReader reader, bool initialState)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |
| System.Boolean | initialState |  |


