---
id: PipePeerConnection.PipeConnectionHandle
title: PipePeerConnection.PipeConnectionHandle
---

# Class PipePeerConnection.PipeConnectionHandle


Virtual connection handle for internal pipe connections.



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
public class PipeConnectionHandle : IConnectionHandle
```


### Properties

#### IsStateful

##### Declaration

```cs
public bool IsStateful { get; }
```
#### SocketLayerConnection

##### Declaration

```cs
public ISocketLayerConnection SocketLayerConnection { get; set; }
```
#### SupportsGracefulDisconnect

##### Declaration

```cs
public bool SupportsGracefulDisconnect { get; }
```
### Methods
#### ToString()



##### Declaration

```cs
public override string ToString()
```

##### Returns
| Type | Description |
| ---- | ---- |
| System.String |  |

#### Disconnect(String)



##### Declaration

```cs
public void Disconnect(string gracefulDisconnectReason)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.String | gracefulDisconnectReason |  |


#### IConnectionHandle.CreateCopy()



##### Declaration

```cs
IConnectionHandle IConnectionHandle.CreateCopy()
```

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.SocketLayer.IConnectionHandle |  |

