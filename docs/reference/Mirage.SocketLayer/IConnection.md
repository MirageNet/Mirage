---
id: IConnection
title: IConnection
---

# Interface IConnection


Connection for 




##### Syntax

```cs
public interface IConnection : ISocketLayerConnection
```


### Properties

#### Handle

##### Declaration

```cs
IConnectionHandle Handle { get; }
```
#### State

##### Declaration

```cs
ConnectionState State { get; }
```
### Methods
#### Disconnect()



##### Declaration

```cs
void Disconnect()
```


#### Disconnect(DisconnectReason)



##### Declaration

```cs
void Disconnect(DisconnectReason reason)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.SocketLayer.DisconnectReason | reason |  |


#### SendNotify(Byte[], Int32, Int32)



##### Declaration

```cs
INotifyToken SendNotify(byte[] packet, int offset, int length)
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

#### SendNotify(Byte[], Int32, Int32, INotifyCallBack)



##### Declaration

```cs
void SendNotify(byte[] packet, int offset, int length, INotifyCallBack callBacks)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Byte[] | packet |  |
| System.Int32 | offset |  |
| System.Int32 | length |  |
| Mirage.SocketLayer.INotifyCallBack | callBacks |  |


#### SendReliable(Byte[], Int32, Int32)


single message, batched by AckSystem



##### Declaration

```cs
void SendReliable(byte[] message, int offset, int length)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Byte[] | message |  |
| System.Int32 | offset |  |
| System.Int32 | length |  |


#### SendUnreliable(Byte[], Int32, Int32)



##### Declaration

```cs
void SendUnreliable(byte[] packet, int offset, int length)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Byte[] | packet |  |
| System.Int32 | offset |  |
| System.Int32 | length |  |


#### FlushBatch()


Forces the connection to send any batched message immediately to the socket

Note: this will only send the packet to the socket. Some sockets may not send on main thread so might not send immediately




##### Declaration

```cs
void FlushBatch()
```


