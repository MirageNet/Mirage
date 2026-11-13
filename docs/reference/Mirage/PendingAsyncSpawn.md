---
id: PendingAsyncSpawn
title: PendingAsyncSpawn
---

# Class PendingAsyncSpawn



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
public class PendingAsyncSpawn : IDisposable
```

### Constructors

#### PendingAsyncSpawn(UInt32)



##### Declaration

```cs
public PendingAsyncSpawn(uint netid)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.UInt32 | netid |  |

### Fields

#### NetId

##### Declaration

```cs
public readonly uint NetId
```

### Properties

#### PendingCount

##### Declaration

```cs
public int PendingCount { get; }
```
### Methods
#### AddMessage(ObjectDestroyMessage)



##### Declaration

```cs
public void AddMessage(ObjectDestroyMessage message)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.ObjectDestroyMessage | message |  |


#### AddMessage(ObjectHideMessage)



##### Declaration

```cs
public void AddMessage(ObjectHideMessage message)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.ObjectHideMessage | message |  |


#### AddMessage(SpawnMessage)



##### Declaration

```cs
public void AddMessage(SpawnMessage message)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.SpawnMessage | message |  |


#### AddMessage(RemoveAuthorityMessage)



##### Declaration

```cs
public void AddMessage(RemoveAuthorityMessage message)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.RemoveAuthorityMessage | message |  |


#### AddMessage(RpcMessage)



##### Declaration

```cs
public void AddMessage(RpcMessage message)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.RemoteCalls.RpcMessage | message |  |


#### AddMessage(RpcWithReplyMessage)



##### Declaration

```cs
public void AddMessage(RpcWithReplyMessage message)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.RemoteCalls.RpcWithReplyMessage | message |  |


#### AddMessage(UpdateVarsMessage)



##### Declaration

```cs
public void AddMessage(UpdateVarsMessage message)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.UpdateVarsMessage | message |  |


#### ApplyAll(ClientObjectManager)



##### Declaration

```cs
public void ApplyAll(ClientObjectManager clientObjectManager)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.ClientObjectManager | clientObjectManager |  |


#### Dispose()



##### Declaration

```cs
public void Dispose()
```


