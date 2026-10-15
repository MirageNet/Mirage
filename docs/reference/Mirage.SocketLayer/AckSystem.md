---
id: AckSystem
title: AckSystem
---

# Class AckSystem



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
public class AckSystem : IDisposable
```

### Constructors

#### AckSystem(IRawConnection, Config, Int32, ITime, Pool&lt;ByteBuffer&gt;, Pool&lt;AckSystem.ReliablePacket&gt;, RingBuffer&lt;AckSystem.AckablePacket&gt;, RingBuffer&lt;AckSystem.ReliableReceived&gt;, Action, ILogger, Metrics)






##### Declaration

```cs
public AckSystem(IRawConnection connection, Config config, int maxPacketSize, ITime time, Pool<ByteBuffer> bufferPool, Pool<AckSystem.ReliablePacket> reliablePool, RingBuffer<AckSystem.AckablePacket> sentAckablePackets, RingBuffer<AckSystem.ReliableReceived> reliableReceive, Action onInvalidPacket, ILogger logger = null, Metrics metrics = null)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.SocketLayer.IRawConnection | connection |  |
| Mirage.SocketLayer.Config | config |  |
| System.Int32 | maxPacketSize |  |
| Mirage.SocketLayer.ITime | time |  |
| Mirage.SocketLayer.Pool&lt;Mirage.SocketLayer.ByteBuffer&gt; | bufferPool |  |
| Mirage.SocketLayer.Pool&lt;Mirage.SocketLayer.AckSystem.ReliablePacket&gt; | reliablePool |  |
| Mirage.SocketLayer.RingBuffer&lt;Mirage.SocketLayer.AckSystem.AckablePacket&gt; | sentAckablePackets |  |
| Mirage.SocketLayer.RingBuffer&lt;Mirage.SocketLayer.AckSystem.ReliableReceived&gt; | reliableReceive |  |
| System.Action | onInvalidPacket |  |
| ILogger | logger |  |
| Mirage.SocketLayer.Metrics | metrics |  |

### Fields

#### SEQUENCE_HEADER

##### Declaration

```cs
public const int SEQUENCE_HEADER = 13
```
#### NOTIFY_HEADER_SIZE
PacketType, sequence, ack sequence, mask

##### Declaration

```cs
public const int NOTIFY_HEADER_SIZE = 13
```
#### RELIABLE_HEADER_SIZE
PacketType, sequence, ack sequence, mask, order

##### Declaration

```cs
public const int RELIABLE_HEADER_SIZE = 15
```
#### ACK_HEADER_SIZE
PacketType, ack sequence, mask

##### Declaration

```cs
public const int ACK_HEADER_SIZE = 11
```
#### FRAGMENT_INDEX_SIZE

##### Declaration

```cs
public const int FRAGMENT_INDEX_SIZE = 1
```
#### MIN_RELIABLE_HEADER_SIZE
Smallest size a header for reliable packet,  + 2 bytes per message

##### Declaration

```cs
public const int MIN_RELIABLE_HEADER_SIZE = 17
```
#### MIN_RELIABLE_FRAGMENT_HEADER_SIZE
Smallest size a header for reliable packet,  + 1 byte for fragment index

##### Declaration

```cs
public const int MIN_RELIABLE_FRAGMENT_HEADER_SIZE = 16
```
#### SizePerFragment

##### Declaration

```cs
public readonly int SizePerFragment
```

### Properties

#### SentAckablePackets

##### Declaration

```cs
public RingBuffer<AckSystem.AckablePacket> SentAckablePackets { get; }
```
#### ReliableReceive

##### Declaration

```cs
public RingBuffer<AckSystem.ReliableReceived> ReliableReceive { get; }
```
### Methods
#### Dispose()



##### Declaration

```cs
public void Dispose()
```


#### NextReliablePacket(out AckSystem.ReliableReceived)


Gets next Reliable packet in order, packet consists for multiple messages
[length, message, length, message, ...]



##### Declaration

```cs
public bool NextReliablePacket(out AckSystem.ReliableReceived packet)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.SocketLayer.AckSystem.ReliableReceived | packet |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean | true if next packet is available |

#### GetNextFragment()



##### Declaration

```cs
public AckSystem.ReliableReceived GetNextFragment()
```

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.SocketLayer.AckSystem.ReliableReceived |  |

#### Update()



##### Declaration

```cs
public void Update()
```


#### SendNotify(Byte[], Int32, Int32)


Use  for non-alloc version



##### Declaration

```cs
public INotifyToken SendNotify(byte[] inPacket, int inOffset, int inLength)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Byte[] | inPacket |  |
| System.Int32 | inOffset |  |
| System.Int32 | inLength |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.SocketLayer.INotifyToken |  |

#### SendNotify(Byte[], Int32, Int32, INotifyCallBack)



##### Declaration

```cs
public void SendNotify(byte[] inPacket, int inOffset, int inLength, INotifyCallBack callBacks)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Byte[] | inPacket |  |
| System.Int32 | inOffset |  |
| System.Int32 | inLength |  |
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


#### ReceiveNotify(ReadOnlySpan&lt;Byte&gt;)


Receives incoming Notify packet
Ignores duplicate or late packets



##### Declaration

```cs
public ReadOnlySpan<byte> ReceiveNotify(ReadOnlySpan<byte> packet)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| ReadOnlySpan&lt;System.Byte&gt; | packet |  |

##### Returns
| Type | Description |
| ---- | ---- |
| ReadOnlySpan&lt;System.Byte&gt; | default or new packet to handle |

#### ReceiveReliable(ReadOnlySpan&lt;Byte&gt;, Boolean)





##### Declaration

```cs
public void ReceiveReliable(ReadOnlySpan<byte> packet, bool isFragment)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| ReadOnlySpan&lt;System.Byte&gt; | packet |  |
| System.Boolean | isFragment |  |


#### ReceiveAck(ReadOnlySpan&lt;Byte&gt;)



##### Declaration

```cs
public void ReceiveAck(ReadOnlySpan<byte> packet)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| ReadOnlySpan&lt;System.Byte&gt; | packet |  |


