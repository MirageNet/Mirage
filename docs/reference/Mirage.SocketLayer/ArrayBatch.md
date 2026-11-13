---
id: ArrayBatch
title: ArrayBatch
---

# Class ArrayBatch



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
public class ArrayBatch : Batch
```

### Constructors

#### ArrayBatch(Int32, Action&lt;Byte[], Int32&gt;, PacketType)



##### Declaration

```cs
public ArrayBatch(int maxPacketSize, Action<byte[], int> send, PacketType reliable)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Int32 | maxPacketSize |  |
| System.Action&lt;System.Byte[], System.Int32&gt; | send |  |
| Mirage.SocketLayer.PacketType | reliable |  |

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


