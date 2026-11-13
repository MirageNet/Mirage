---
id: PooledNetworkReader
title: PooledNetworkReader
---

# Class PooledNetworkReader


NetworkReader to be used with 



<div class="inheritance">

##### Inheritance

<div class="level" style={{"--data-index": 0}}>
System.Object
</div>
<div class="level" style={{"--data-index": 1}}>
Mirage.Serialization.NetworkReader
</div>
<div class="level" style={{"--data-index": 2}}>
Mirage.Serialization.MirageNetworkReader
</div>
</div>

##### Inherited Members

<details>
<summary>Show</summary>

Mirage.Serialization.MirageNetworkReader.ObjectLocator


Mirage.Serialization.NetworkReader.StringStore


Mirage.Serialization.NetworkReader.BitLength


Mirage.Serialization.NetworkReader.BitPosition


Mirage.Serialization.NetworkReader.BytePosition


Mirage.Serialization.NetworkReader.Dispose()


Mirage.Serialization.NetworkReader.CanRead()


Mirage.Serialization.NetworkReader.PadToByte()


Mirage.Serialization.NetworkReader.ReadBoolean()


Mirage.Serialization.NetworkReader.ReadBooleanAsUlong()


Mirage.Serialization.NetworkReader.ReadSByte()


Mirage.Serialization.NetworkReader.ReadByte()


Mirage.Serialization.NetworkReader.ReadInt16()


Mirage.Serialization.NetworkReader.ReadUInt16()


Mirage.Serialization.NetworkReader.ReadInt32()


Mirage.Serialization.NetworkReader.ReadUInt32()


Mirage.Serialization.NetworkReader.ReadInt64()


Mirage.Serialization.NetworkReader.ReadUInt64()


Mirage.Serialization.NetworkReader.ReadSingle()


Mirage.Serialization.NetworkReader.ReadDouble()


Mirage.Serialization.NetworkReader.PadAndCopy&lt;T&gt;(T)

</details>

##### Syntax

```cs
public sealed class PooledNetworkReader : MirageNetworkReader, IDisposable
```

### Methods
#### CreateNew(Pool&lt;PooledNetworkReader&gt;)



##### Declaration

```cs
public static PooledNetworkReader CreateNew(Pool<PooledNetworkReader> pool)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.SocketLayer.Pool&lt;Mirage.Serialization.PooledNetworkReader&gt; | pool |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.Serialization.PooledNetworkReader |  |

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


#### Dispose(Boolean)



##### Declaration

```cs
protected override void Dispose(bool disposing)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Boolean | disposing |  |


