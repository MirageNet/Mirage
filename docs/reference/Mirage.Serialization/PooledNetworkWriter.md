---
id: PooledNetworkWriter
title: PooledNetworkWriter
---

# Class PooledNetworkWriter


NetworkWriter to be used with 



<div class="inheritance">

##### Inheritance

<div class="level" style={{"--data-index": 0}}>
System.Object
</div>
<div class="level" style={{"--data-index": 1}}>
Mirage.Serialization.NetworkWriter
</div>
</div>

##### Inherited Members

<details>
<summary>Show</summary>

Mirage.Serialization.NetworkWriter.StringStore


Mirage.Serialization.NetworkWriter.ByteCapacity


Mirage.Serialization.NetworkWriter.ByteLength


Mirage.Serialization.NetworkWriter.BitPosition


Mirage.Serialization.NetworkWriter.Reset()


Mirage.Serialization.NetworkWriter.ToArray()


Mirage.Serialization.NetworkWriter.ToArraySegment()


Mirage.Serialization.NetworkWriter.PadToByte()


Mirage.Serialization.NetworkWriter.PadAndCopy&lt;T&gt;(T)


Mirage.Serialization.NetworkWriter.CopyFromWriter(Mirage.Serialization.NetworkWriter)

</details>

##### Syntax

```cs
public sealed class PooledNetworkWriter : NetworkWriter, IDisposable
```

### Methods
#### CreateNew(Int32, Pool&lt;PooledNetworkWriter&gt;)



##### Declaration

```cs
public static PooledNetworkWriter CreateNew(int bufferSize, Pool<PooledNetworkWriter> pool)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Int32 | bufferSize |  |
| Mirage.SocketLayer.Pool&lt;Mirage.Serialization.PooledNetworkWriter&gt; | pool |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.Serialization.PooledNetworkWriter |  |

#### Release()


Puts object back in Pool



##### Declaration

```cs
public void Release()
```


#### IDisposable.Dispose()



##### Declaration

```cs
void IDisposable.Dispose()
```


