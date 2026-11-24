---
id: PipePeerConnection
title: PipePeerConnection
---

# Class PipePeerConnection


A  that directly sends data to a 
bypassing the transport layer for local host/client communication.



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
public class PipePeerConnection : IConnection, ISocketLayerConnection
```


### Properties

#### IConnection.Handle

##### Declaration

```cs
IConnectionHandle IConnection.Handle { get; }
```
#### State

##### Declaration

```cs
public ConnectionState State { get; }
```
### Methods
#### Create(IDataHandler, IDataHandler, Action, Action)



##### Declaration

```cs
public static (IConnection clientConn, IConnection serverConn) Create(IDataHandler clientHandler, IDataHandler serverHandler, Action clientOnDisconnect, Action serverOnDisconnect)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.SocketLayer.IDataHandler | clientHandler |  |
| Mirage.SocketLayer.IDataHandler | serverHandler |  |
| System.Action | clientOnDisconnect |  |
| System.Action | serverOnDisconnect |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.ValueTuple{Mirage.SocketLayer.IConnection,Mirage.SocketLayer.IConnection} |  |

#### ToString()



##### Declaration

```cs
public override string ToString()
```

##### Returns
| Type | Description |
| ---- | ---- |
| System.String |  |

#### IConnection.FlushBatch()



##### Declaration

```cs
void IConnection.FlushBatch()
```


#### IConnection.Disconnect()



##### Declaration

```cs
void IConnection.Disconnect()
```


#### IConnection.Disconnect(DisconnectReason)



##### Declaration

```cs
void IConnection.Disconnect(DisconnectReason reason)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.SocketLayer.DisconnectReason | reason |  |


#### SendNotify(Byte[], Int32, Int32)



##### Declaration

```cs
public INotifyToken SendNotify(byte[] packet, int offset, int length)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Byte[] | packet |  |
| System.Int32 | offset |  |
| System.Int32 | length |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.SocketLayer.INotifyToken |  |

#### SendNotify(ArraySegment&lt;Byte&gt;)



##### Declaration

```cs
public INotifyToken SendNotify(ArraySegment<byte> packet)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.ArraySegment&lt;System.Byte&gt; | packet |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.SocketLayer.INotifyToken |  |

#### SendNotify(Byte[])



##### Declaration

```cs
public INotifyToken SendNotify(byte[] packet)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Byte[] | packet |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.SocketLayer.INotifyToken |  |

#### SendNotify(Byte[], Int32, Int32, INotifyCallBack)



##### Declaration

```cs
public void SendNotify(byte[] packet, int offset, int length, INotifyCallBack callBacks)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Byte[] | packet |  |
| System.Int32 | offset |  |
| System.Int32 | length |  |
| Mirage.SocketLayer.INotifyCallBack | callBacks |  |


#### SendNotify(ArraySegment&lt;Byte&gt;, INotifyCallBack)



##### Declaration

```cs
public void SendNotify(ArraySegment<byte> packet, INotifyCallBack callBacks)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.ArraySegment&lt;System.Byte&gt; | packet |  |
| Mirage.SocketLayer.INotifyCallBack | callBacks |  |


#### SendNotify(Byte[], INotifyCallBack)



##### Declaration

```cs
public void SendNotify(byte[] packet, INotifyCallBack callBacks)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Byte[] | packet |  |
| Mirage.SocketLayer.INotifyCallBack | callBacks |  |


#### SendReliable(Byte[], Int32, Int32)



##### Declaration

```cs
public void SendReliable(byte[] message, int offset, int length)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Byte[] | message |  |
| System.Int32 | offset |  |
| System.Int32 | length |  |


#### SendReliable(ArraySegment&lt;Byte&gt;)



##### Declaration

```cs
public void SendReliable(ArraySegment<byte> message)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.ArraySegment&lt;System.Byte&gt; | message |  |


#### SendReliable(Byte[])



##### Declaration

```cs
public void SendReliable(byte[] message)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Byte[] | message |  |


#### SendUnreliable(Byte[], Int32, Int32)



##### Declaration

```cs
public void SendUnreliable(byte[] packet, int offset, int length)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Byte[] | packet |  |
| System.Int32 | offset |  |
| System.Int32 | length |  |


#### SendUnreliable(ArraySegment&lt;Byte&gt;)



##### Declaration

```cs
public void SendUnreliable(ArraySegment<byte> packet)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.ArraySegment&lt;System.Byte&gt; | packet |  |


#### SendUnreliable(Byte[])



##### Declaration

```cs
public void SendUnreliable(byte[] packet)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Byte[] | packet |  |


