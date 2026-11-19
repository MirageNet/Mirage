---
id: ReliableBatch
title: ReliableBatch
---

# Class ReliableBatch



<div class="inheritance">

##### Inheritance

<div class="level" style={{"--data-index": 0}}>
System.Object
</div>
<div class="level" style={{"--data-index": 1}}>
Mirage.SocketLayer.Batch
</div>
</div>

##### Inherited Members

<details>
<summary>Show</summary>

Mirage.SocketLayer.Batch.MESSAGE_LENGTH_SIZE


Mirage.SocketLayer.Batch.Flush()

</details>

##### Syntax

```cs
public class ReliableBatch : Batch, IDisposable
```

### Constructors

#### ReliableBatch(Int32, Func&lt;PacketType, AckSystem.ReliablePacket&gt;, Action&lt;AckSystem.ReliablePacket&gt;)



##### Declaration

```cs
public ReliableBatch(int maxPacketSize, Func<PacketType, AckSystem.ReliablePacket> createReliableBuffer, Action<AckSystem.ReliablePacket> sendReliablePacket)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Int32 | maxPacketSize |  |
| System.Func&lt;Mirage.SocketLayer.PacketType, Mirage.SocketLayer.AckSystem.ReliablePacket&gt; | createReliableBuffer |  |
| System.Action&lt;Mirage.SocketLayer.AckSystem.ReliablePacket&gt; | sendReliablePacket |  |

### Properties

#### Created

##### Declaration

```cs
protected override bool Created { get; }
```
### Methods
#### GetBatch()



##### Declaration

```cs
protected override byte[] GetBatch()
```

##### Returns
| Type | Description |
| ---- | ---- |
| System.Byte[] |  |

#### GetBatchLength()



##### Declaration

```cs
protected override int GetBatchLength()
```

##### Returns
| Type | Description |
| ---- | ---- |
| System.Int32 |  |

#### CreateNewBatch()



##### Declaration

```cs
protected override void CreateNewBatch()
```


#### SendAndReset()



##### Declaration

```cs
protected override void SendAndReset()
```


#### IDisposable.Dispose()



##### Declaration

```cs
void IDisposable.Dispose()
```


