---
id: ISyncObject
title: ISyncObject
---

# Interface ISyncObject


A sync object is an object that can synchronize it&apos;s state
between server and client, such as a SyncList




##### Syntax

```cs
public interface ISyncObject
```


### Properties

#### IsDirty

true if there are changes since the last flush


##### Declaration

```cs
bool IsDirty { get; }
```
### Methods
#### SetShouldSyncFrom(Boolean)


Are we sending or receiving data from this instance. This is used to determine if we should throw if a change is made on the wrong instance



##### Declaration

```cs
void SetShouldSyncFrom(bool shouldSync)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Boolean | shouldSync |  |


#### Flush()


Discard all the queued changes
Consider the object fully synchronized with clients



##### Declaration

```cs
void Flush()
```


#### OnSerializeAll(NetworkWriter)


Write a full copy of the object



##### Declaration

```cs
void OnSerializeAll(NetworkWriter writer)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |


#### OnSerializeDelta(NetworkWriter)


Write the changes made to the object since last sync



##### Declaration

```cs
void OnSerializeDelta(NetworkWriter writer)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |


#### OnDeserializeAll(NetworkReader)


Reads a full copy of the object



##### Declaration

```cs
void OnDeserializeAll(NetworkReader reader)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |


#### OnDeserializeDelta(NetworkReader)


Reads the changes made to the object since last sync



##### Declaration

```cs
void OnDeserializeDelta(NetworkReader reader)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |


#### Reset()


Resets the SyncObject so that it can be re-used



##### Declaration

```cs
void Reset()
```


#### SetNetworkBehaviour(NetworkBehaviour)


Sets the NetworkBehaviour that owns this SyncObject
This can be used by custom syncObjects to listen to events on NetworkBehaviour
This will only be called once, the first time the object is spawned (similar to unity&apos;s awake call)



##### Declaration

```cs
void SetNetworkBehaviour(NetworkBehaviour networkBehaviour)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.NetworkBehaviour | networkBehaviour |  |


