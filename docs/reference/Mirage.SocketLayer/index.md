---
id: Mirage.SocketLayer
title: Mirage.SocketLayer
---

# Mirage.SocketLayer

## Classes

#### [AckSystem](./AckSystem)
#### [AckSystem.ReliablePacket](./AckSystem.ReliablePacket)
#### [ArrayBatch](./ArrayBatch)
#### [AutoCompleteToken](./AutoCompleteToken)
> 
Token that invokes  immediately

#### [Batch](./Batch)
#### [BufferFullException](./BufferFullException)
#### [ByteBuffer](./ByteBuffer)
> 
Warpper around a byte[] that belongs to a 

#### [ByteUtils](./ByteUtils)
#### [Config](./Config)
#### [ConnectionExtensions](./ConnectionExtensions)
#### [INotifyCallBackExtensions](./INotifyCallBackExtensions)
#### [MessageSizeException](./MessageSizeException)
#### [Metrics](./Metrics)
#### [NoConnectionException](./NoConnectionException)
#### [NotifyToken](./NotifyToken)
> 
Object returned from  with events for when packet is Lost or Delivered

#### [NotifyTokenException](./NotifyTokenException)
#### [Peer](./Peer)
> 
Controls flow of data in/out of mirage, Uses 

#### [Pool&lt;T&gt;](./Pool-1)
> 
Holds a collection of  so they can be re-used without allocations

#### [RateLimitBucket](./RateLimitBucket)
#### [ReliableBatch](./ReliableBatch)
#### [RingBuffer&lt;T&gt;](./RingBuffer-1)
#### [Sequencer](./Sequencer)
> 
A sequence generator that can wrap.
For example a 2 bit sequencer would generate
the following numbers:
    0,1,2,3,0,1,2,3,0,1,2,3...

#### [SocketFactory](./SocketFactory)
> 
Creates an instance of 

#### [SocketLayerException](./SocketLayerException)
> 
Base Exception by all errors from using SocketLayer

## Structs

#### [AckSystem.AckablePacket](./AckSystem.AckablePacket)
#### [AckSystem.ReliableReceived](./AckSystem.ReliableReceived)
#### [Metrics.Frame](./Metrics.Frame)
#### [PoolMetrics](./PoolMetrics)
#### [RateLimitBucket.RefillConfig](./RateLimitBucket.RefillConfig)
#### [RingBuffer&lt;T&gt;.Option](./RingBuffer-1.Option)
## Interfaces

#### [IBindEndPoint](./IBindEndPoint)
#### [IConnectEndPoint](./IConnectEndPoint)
#### [IConnection](./IConnection)
> 
Connection for 

#### [IConnectionHandle](./IConnectionHandle)
> 
Object that can be used as an endPoint or handle for  and 

Implementation of this should override  and  so that 2 instance wil be equal if they have the same address internally


When a new connection is received by Peer a copy of this endPoint will be created and given to that connection.
On future received the incoming endPoint will be compared to active connections inside a dictionary


#### [IDataHandler](./IDataHandler)
> 
Handles data from SocketLayer
A high level script should implement this interface give it to Peer when it is created

#### [IHasAddress](./IHasAddress)
> 
Can be added to SocketFactory that have an Address Setting

#### [IHasPort](./IHasPort)
> 
Can be added to SocketFactory that have a Port Setting

#### [INotifyCallBack](./INotifyCallBack)
> 
Can be passed into  and methods will be invoked when notify is delivered or lost

See the Notify Example on how to use this interface


#### [INotifyToken](./INotifyToken)
> 
Object returned from  with events for when packet is Lost or Delivered

#### [IPeer](./IPeer)
#### [IRawConnection](./IRawConnection)
> 
A connection that can send data directly to sockets
Only things inside socket layer should be sending raw packets. Others should use the methods inside 

#### [ISocket](./ISocket)
> 
Link between Mirage and the outside world

#### [ISocketLayerConnection](./ISocketLayerConnection)
> 
Used by  to get get the  object directly to avoid lookup

#### [ITime](./ITime)
## Enums

#### [Commands](./Commands)
> 
Small message used to control a connection

 and Commands uses their own byte/enum to split up the flow and add struture to the code.


#### [ConnectionState](./ConnectionState)
#### [DisconnectReason](./DisconnectReason)
> 
Reason why a connection was disconnected

#### [PacketType](./PacketType)
#### [PeerConfigProfile](./PeerConfigProfile)
#### [RejectReason](./RejectReason)
> 
Reason for reject sent from server

## Delegates

#### [OnData](./OnData)
> 
Delegate for handling incoming data from a connection.
Should only be invoked from within .

#### [OnDisconnect](./OnDisconnect)
> 
Delegate for handling a disconnection from a connection.
Should only be invoked from within .

#### [Pool&lt;T&gt;.CreateNewItem](./Pool-1.CreateNewItem)
#### [Pool&lt;T&gt;.CreateNewItemNoCount](./Pool-1.CreateNewItemNoCount)
