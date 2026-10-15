---
id: NetworkBehaviour
title: NetworkBehaviour
---

# Class NetworkBehaviour


Base class which should be inherited by scripts which contain networking functionality.




<div class="inheritance">

##### Inheritance

<div class="level" style={{"--data-index": 0}}>
System.Object
</div>
</div>

##### Syntax

```cs
public abstract class NetworkBehaviour : MonoBehaviour
```


### Fields

#### _nextSyncTime

##### Declaration

```cs
protected double _nextSyncTime
```
#### SyncSettings

Sync settings for this NetworkBehaviour
Settings will be hidden in inspector unless Behaviour has SyncVar or SyncObjects


##### Declaration

```cs
public SyncSettings SyncSettings
```
#### syncObjects

objects that can synchronize themselves, such as synclists


##### Declaration

```cs
protected readonly List<ISyncObject> syncObjects
```
#### COMPONENT_INDEX_NOT_FOUND

##### Declaration

```cs
public const int COMPONENT_INDEX_NOT_FOUND = -1
```

### Properties

#### IsServer

Returns true if this object is active on an active server.
This is only true if the object has been spawned. This is different from NetworkServer.active, which is true if the server itself is active rather than this object being active.


##### Declaration

```cs
public bool IsServer { get; }
```
#### IsClient

Returns true if running as a client and this object was spawned by a server.


##### Declaration

```cs
public bool IsClient { get; }
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
In multiplayer games, there are multiple instances of the Player object. The client needs to know which one is for &quot;themselves&quot; so that only that player processes input and potentially has a camera attached. The IsLocalPlayer function will return true only for the player instance that belongs to the player on the local machine, so it can be used to filter out input for non-local players.


##### Declaration

```cs
public bool IsLocalPlayer { get; }
```
#### IsServerOnly

True if this object only exists on the server


##### Declaration

```cs
public bool IsServerOnly { get; }
```
#### IsClientOnly

True if this object exists on a client that is not also acting as a server


##### Declaration

```cs
public bool IsClientOnly { get; }
```
#### HasAuthority

This returns true if this object is the authoritative version of the object in the distributed network application.
The  value on the NetworkIdentity determines how authority is determined. For most objects, authority is held by the server. For objects with  set, authority is held by the client of that player.


##### Declaration

```cs
public bool HasAuthority { get; }
```
#### NetId

The unique network Id of this object.
This is assigned at runtime by the network server and will be unique for all objects for that network session.


##### Declaration

```cs
public uint NetId { get; }
```
#### Server

The  associated to this object.


##### Declaration

```cs
public NetworkServer Server { get; }
```
#### ServerObjectManager

Quick Reference to the NetworkIdentities ServerObjectManager. Present only for server/host instances.


##### Declaration

```cs
public ServerObjectManager ServerObjectManager { get; }
```
#### Client

The  associated to this object.


##### Declaration

```cs
public NetworkClient Client { get; }
```
#### ClientObjectManager

Quick Reference to the NetworkIdentities ClientObjectManager. Present only for instances instances.


##### Declaration

```cs
public ClientObjectManager ClientObjectManager { get; }
```
#### Owner

The  associated with this  This is only valid for player objects on the server.


##### Declaration

```cs
public INetworkPlayer Owner { get; }
```
#### World

##### Declaration

```cs
public NetworkWorld World { get; }
```
#### NetworkTime

Returns the appropriate NetworkTime instance based on if this NetworkBehaviour is running as a Server or Client.


##### Declaration

```cs
public NetworkTime NetworkTime { get; }
```
#### BehaviourId

Get Id of this NetworkBehaviour, Its NetId and ComponentIndex


##### Declaration

```cs
public NetworkBehaviour.Id BehaviourId { get; }
```
#### SyncVarDirtyBits

##### Declaration

```cs
protected ulong SyncVarDirtyBits { get; }
```
#### AnySyncObjectDirty

##### Declaration

```cs
protected bool AnySyncObjectDirty { get; }
```
#### Identity

Returns the NetworkIdentity of this object


##### Declaration

```cs
public NetworkIdentity Identity { get; }
```
#### ComponentIndex

Returns the index of the component on this object


##### Declaration

```cs
public int ComponentIndex { get; }
```
### Methods
#### GetSyncVarHookGuard(UInt64)



##### Declaration

```cs
protected bool GetSyncVarHookGuard(ulong dirtyBit)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.UInt64 | dirtyBit |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean |  |

#### SetSyncVarHookGuard(UInt64, Boolean)



##### Declaration

```cs
protected void SetSyncVarHookGuard(ulong dirtyBit, bool value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.UInt64 | dirtyBit |  |
| System.Boolean | value |  |


#### InitSyncObject(ISyncObject)



##### Declaration

```cs
protected void InitSyncObject(ISyncObject syncObject)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Collections.ISyncObject | syncObject |  |


#### UpdateSyncObjectShouldSync()


Call this after updating SyncSettings to update all SyncObjects

This only needs to be called manually if updating syncSettings at runtime.
Mirage will automatically call this after serializing or deserializing with initialState




##### Declaration

```cs
public void UpdateSyncObjectShouldSync()
```


#### SyncVarEqual&lt;T&gt;(T, T)



##### Declaration

```cs
protected bool SyncVarEqual<T>(T value, T fieldValue)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| T | value |  |
| T | fieldValue |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean |  |

#### SetDirtyBit(UInt64)


Used to set the behaviour as dirty, so that a network update will be sent for the object.
these are masks, not bit numbers, ie. 0x004 not 2



##### Declaration

```cs
public void SetDirtyBit(ulong bitMask)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.UInt64 | bitMask | Bit mask to set. |


#### ClearDirtyBit(UInt64)


Used to clear dirty bit.
Object may still be in dirty list, so will be checked in next update. but values in this mask will no longer be set until they are changed again



##### Declaration

```cs
public void ClearDirtyBit(ulong bitMask)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.UInt64 | bitMask | Bit mask to set. |


#### ClearDirtyBits()



##### Declaration

```cs
public void ClearDirtyBits()
```


#### ClearShouldSync(Double)


Clears dirty bits and sets the next sync time



##### Declaration

```cs
public void ClearShouldSync(double now)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Double | now |  |


#### ShouldSync(Double)


True if this behaviour is dirty and it is time to sync



##### Declaration

```cs
public bool ShouldSync(double time)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Double | time |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean |  |

#### TimeToSync(Double)


If it is time to sync based on last sync and 



##### Declaration

```cs
public bool TimeToSync(double time)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Double | time |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean |  |

#### AnyDirtyBits()


Are any SyncVar or SyncObjects dirty



##### Declaration

```cs
public bool AnyDirtyBits()
```

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean |  |

#### OnSerialize(NetworkWriter, Boolean)


Virtual function to override to send custom serialization data. The corresponding function to send serialization data is OnDeserialize().



##### Declaration

```cs
public virtual bool OnSerialize(NetworkWriter writer, bool initialState)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer | Writer to use to write to the stream. |
| System.Boolean | initialState | If this is being called to send initial state. |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean | True if data was written. |

#### OnDeserialize(NetworkReader, Boolean)


Virtual function to override to receive custom serialization data. The corresponding function to send serialization data is OnSerialize().



##### Declaration

```cs
public virtual void OnDeserialize(NetworkReader reader, bool initialState)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader | Reader to read from the stream. |
| System.Boolean | initialState | True if being sent initial state. |


#### SerializeSyncVars(NetworkWriter, Boolean)



##### Declaration

```cs
public virtual bool SerializeSyncVars(NetworkWriter writer, bool initialState)
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

#### DeserializeSyncVars(NetworkReader, Boolean)



##### Declaration

```cs
public virtual void DeserializeSyncVars(NetworkReader reader, bool initialState)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |
| System.Boolean | initialState |  |


#### SetDeserializeMask(UInt64, Int32)



##### Declaration

```cs
protected void SetDeserializeMask(ulong dirtyBit, int offset)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.UInt64 | dirtyBit |  |
| System.Int32 | offset |  |


#### SerializeObjects(NetworkWriter, Boolean)



##### Declaration

```cs
public bool SerializeObjects(NetworkWriter writer, bool initialState)
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

#### SerializeObjectsAll(NetworkWriter)



##### Declaration

```cs
public bool SerializeObjectsAll(NetworkWriter writer)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean |  |

#### SerializeObjectsDelta(NetworkWriter)



##### Declaration

```cs
public bool SerializeObjectsDelta(NetworkWriter writer)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean |  |

#### GetRpcCount()



##### Declaration

```cs
protected virtual int GetRpcCount()
```

##### Returns
| Type | Description |
| ---- | ---- |
| System.Int32 |  |

#### RegisterRpc(RemoteCallCollection)



##### Declaration

```cs
protected virtual void RegisterRpc(RemoteCallCollection collection)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.RemoteCalls.RemoteCallCollection | collection |  |


