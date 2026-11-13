---
id: NetworkWriter
title: NetworkWriter
---

# Class NetworkWriter


Bit writer, writes values to a buffer on a bit level
Use  to reduce memory allocation



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
public class NetworkWriter
```

### Constructors

#### NetworkWriter(Int32)



##### Declaration

```cs
public NetworkWriter(int minByteCapacity)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Int32 | minByteCapacity |  |

#### NetworkWriter(Int32, Boolean)



##### Declaration

```cs
public NetworkWriter(int minByteCapacity, bool allowResize)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Int32 | minByteCapacity |  |
| System.Boolean | allowResize |  |

### Fields

#### StringStore

##### Declaration

```cs
public StringStore StringStore
```

### Properties

#### ByteCapacity

Size limit of buffer


##### Declaration

```cs
public int ByteCapacity { get; }
```
#### ByteLength

Current  rounded up to nearest multiple of 8
To set byte position use  multiple by 8


##### Declaration

```cs
public int ByteLength { get; }
```
#### BitPosition

Current bit position for writing to buffer
To set bit position use 


##### Declaration

```cs
public int BitPosition { get; }
```
### Methods
#### Finalize()



##### Declaration

```cs
protected void Finalize()
```


#### Reset()



##### Declaration

```cs
public void Reset()
```


#### ToArray()


Copies internal buffer to new Array
To reduce Allocations use  instead



##### Declaration

```cs
public byte[] ToArray()
```

##### Returns
| Type | Description |
| ---- | ---- |
| System.Byte[] |  |

#### ToArraySegment()



##### Declaration

```cs
public ArraySegment<byte> ToArraySegment()
```

##### Returns
| Type | Description |
| ---- | ---- |
| System.ArraySegment&lt;System.Byte&gt; |  |

#### PadToByte()



##### Declaration

```cs
public void PadToByte()
```


#### WriteBoolean(Boolean)



##### Declaration

```cs
public void WriteBoolean(bool value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Boolean | value |  |


#### WriteBoolean(UInt64)


Writes first bit of value to buffer



##### Declaration

```cs
public void WriteBoolean(ulong value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.UInt64 | value |  |


#### WriteSByte(SByte)



##### Declaration

```cs
public void WriteSByte(sbyte value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.SByte | value |  |


#### WriteByte(Byte)



##### Declaration

```cs
public void WriteByte(byte value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Byte | value |  |


#### WriteInt16(Int16)



##### Declaration

```cs
public void WriteInt16(short value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Int16 | value |  |


#### WriteUInt16(UInt16)



##### Declaration

```cs
public void WriteUInt16(ushort value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.UInt16 | value |  |


#### WriteInt32(Int32)



##### Declaration

```cs
public void WriteInt32(int value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Int32 | value |  |


#### WriteUInt32(UInt32)



##### Declaration

```cs
public void WriteUInt32(uint value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.UInt32 | value |  |


#### WriteInt64(Int64)



##### Declaration

```cs
public void WriteInt64(long value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Int64 | value |  |


#### WriteUInt64(UInt64)



##### Declaration

```cs
public void WriteUInt64(ulong value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.UInt64 | value |  |


#### WriteSingle(Single)



##### Declaration

```cs
public void WriteSingle(float value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Single | value |  |


#### WriteDouble(Double)



##### Declaration

```cs
public void WriteDouble(double value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Double | value |  |


#### Write(UInt64, Int32)



##### Declaration

```cs
public void Write(ulong value, int bits)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.UInt64 | value |  |
| System.Int32 | bits |  |


#### WriteAtBytePosition(UInt64, Int32, Int32)


Same as  expect position given is in bytes instead of bits
WARNING: When writing to bytes instead of bits make sure you are able to read at the right position when deserializing as it might cause data to be misaligned



##### Declaration

```cs
public void WriteAtBytePosition(ulong value, int bits, int bytePosition)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.UInt64 | value |  |
| System.Int32 | bits |  |
| System.Int32 | bytePosition |  |


#### WriteAtPosition(UInt64, Int32, Int32)


Writes n bits from value to bitPosition
This methods can be used to go back to a previous position to write length or other flags to the buffer after other data has been written
WARNING: This method does not change the internal position so will not change the overall length if writing past internal position



##### Declaration

```cs
public void WriteAtPosition(ulong value, int bits, int bitPosition)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.UInt64 | value | value to write |
| System.Int32 | bits | number of bits in value to write |
| System.Int32 | bitPosition | where to write bits |


#### MoveBitPosition(Int32)


Moves the internal bit position
For most usecases it is safer to use 
WARNING: When writing to earlier position make sure to move position back to end of buffer after writing because position is also used as length



##### Declaration

```cs
public void MoveBitPosition(int newPosition)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Int32 | newPosition |  |


#### PadAndCopy&lt;T&gt;(T)



   Moves position to nearest byte then copies struct to that position




##### Declaration

```cs
public void PadAndCopy<T>(in T value)
    where T : struct
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| T | value |  |


#### WriteBytes(Byte[], Int32, Int32)


Moves position to nearest byte then writes bytes to that position



##### Declaration

```cs
public void WriteBytes(byte[] array, int offset, int length)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Byte[] | array |  |
| System.Int32 | offset |  |
| System.Int32 | length |  |


#### WriteSpanRaw(ReadOnlySpan&lt;Byte&gt;)


Moves position to nearest byte then copies span to that position



##### Declaration

```cs
public void WriteSpanRaw(ReadOnlySpan<byte> span)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| ReadOnlySpan&lt;System.Byte&gt; | span |  |


#### CopyFromWriter(NetworkWriter)


Copies all data from other



##### Declaration

```cs
public void CopyFromWriter(NetworkWriter other)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | other |  |


#### CopyFromWriter(NetworkWriter, Int32, Int32)


Copies bitLength bits from other starting at otherBitPosition



##### Declaration

```cs
public void CopyFromWriter(NetworkWriter other, int otherBitPosition, int bitLength)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | other |  |
| System.Int32 | otherBitPosition |  |
| System.Int32 | bitLength |  |


#### CopyFromPointer(Void*, Int32, Int32)


Copies bitLength bits from ptr starting at otherBitPosition



##### Declaration

```cs
public void CopyFromPointer(void *ptr, int otherBitPosition, int bitLength)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Void* | ptr |  |
| System.Int32 | otherBitPosition |  |
| System.Int32 | bitLength |  |


