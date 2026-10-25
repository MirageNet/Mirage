---
id: MirageNetworkReader
title: MirageNetworkReader
---

# Class MirageNetworkReader


NetworkReader but has a ObjectLocator field that can be used by Reader functions to fetch NetworkIdentity



<div class="inheritance">

##### Inheritance

<div class="level" style={{"--data-index": 0}}>
System.Object
</div>
<div class="level" style={{"--data-index": 1}}>
Mirage.Serialization.NetworkReader
</div>
</div>

##### Inherited Members

<details>
<summary>Show</summary>

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
public class MirageNetworkReader : NetworkReader, IDisposable
```


### Properties

#### ObjectLocator

Used to find objects by net id


##### Declaration

```cs
public IObjectLocator ObjectLocator { get; set; }
```
### Methods
#### Dispose(Boolean)



##### Declaration

```cs
protected override void Dispose(bool disposing)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Boolean | disposing |  |


