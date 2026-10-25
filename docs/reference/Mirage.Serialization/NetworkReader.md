---
id: NetworkReader
title: NetworkReader
---

# Class NetworkReader


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
public class NetworkReader : IDisposable
```

### Constructors

#### NetworkReader()



##### Declaration

```cs
public NetworkReader()
```

### Fields

#### StringStore

##### Declaration

```cs
public StringStore StringStore
```

### Properties

#### BitLength

Size of buffer that is being read from


##### Declaration

```cs
public int BitLength { get; }
```
#### BitPosition

Current bit position for reading from buffer


##### Declaration

```cs
public int BitPosition { get; }
```
#### BytePosition

Current  rounded up to nearest multiple of 8


##### Declaration

```cs
public int BytePosition { get; }
```
### Methods
#### Finalize()



##### Declaration

```cs
protected void Finalize()
```


#### Dispose(Boolean)



##### Declaration

```cs
protected virtual void Dispose(bool disposing)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Boolean | disposing | true if called from IDisposable |


#### Dispose()



##### Declaration

```cs
public void Dispose()
```


#### Reset(ArraySegment&lt;Byte&gt;)



##### Declaration

```cs
public void Reset(ArraySegment<byte> segment)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.ArraySegment&lt;System.Byte&gt; | segment |  |


#### Reset(Byte[])



##### Declaration

```cs
public void Reset(byte[] array)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Byte[] | array |  |


#### Reset(Byte[], Int32, Int32)



##### Declaration

```cs
public void Reset(byte[] array, int position, int length)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Byte[] | array |  |
| System.Int32 | position |  |
| System.Int32 | length |  |


#### CanRead()


Can read atleast 1 bit



##### Declaration

```cs
public bool CanRead()
```

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean |  |

#### CanReadBits(Int32)


Can atleast readCount bits



##### Declaration

```cs
public bool CanReadBits(int readCount)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Int32 | readCount |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean |  |

#### CanReadBytes(Int32)


Can atleast readCount bytes



##### Declaration

```cs
public bool CanReadBytes(int readCount)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Int32 | readCount |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean |  |

#### PadToByte()



##### Declaration

```cs
public void PadToByte()
```


#### ReadBoolean()



##### Declaration

```cs
public bool ReadBoolean()
```

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean |  |

#### ReadBooleanAsUlong()


Writes first bit of value to buffer



##### Declaration

```cs
public ulong ReadBooleanAsUlong()
```

##### Returns
| Type | Description |
| ---- | ---- |
| System.UInt64 |  |

#### ReadSByte()



##### Declaration

```cs
public sbyte ReadSByte()
```

##### Returns
| Type | Description |
| ---- | ---- |
| System.SByte |  |

#### ReadByte()



##### Declaration

```cs
public byte ReadByte()
```

##### Returns
| Type | Description |
| ---- | ---- |
| System.Byte |  |

#### ReadInt16()



##### Declaration

```cs
public short ReadInt16()
```

##### Returns
| Type | Description |
| ---- | ---- |
| System.Int16 |  |

#### ReadUInt16()



##### Declaration

```cs
public ushort ReadUInt16()
```

##### Returns
| Type | Description |
| ---- | ---- |
| System.UInt16 |  |

#### ReadInt32()



##### Declaration

```cs
public int ReadInt32()
```

##### Returns
| Type | Description |
| ---- | ---- |
| System.Int32 |  |

#### ReadUInt32()



##### Declaration

```cs
public uint ReadUInt32()
```

##### Returns
| Type | Description |
| ---- | ---- |
| System.UInt32 |  |

#### ReadInt64()



##### Declaration

```cs
public long ReadInt64()
```

##### Returns
| Type | Description |
| ---- | ---- |
| System.Int64 |  |

#### ReadUInt64()



##### Declaration

```cs
public ulong ReadUInt64()
```

##### Returns
| Type | Description |
| ---- | ---- |
| System.UInt64 |  |

#### ReadSingle()



##### Declaration

```cs
public float ReadSingle()
```

##### Returns
| Type | Description |
| ---- | ---- |
| System.Single |  |

#### ReadDouble()



##### Declaration

```cs
public double ReadDouble()
```

##### Returns
| Type | Description |
| ---- | ---- |
| System.Double |  |

#### Read(Int32)



##### Declaration

```cs
public ulong Read(int bits)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Int32 | bits |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.UInt64 |  |

#### ReadAtPosition(Int32, Int32)


Reads n bits from buffer at bitPosition



##### Declaration

```cs
public ulong ReadAtPosition(int bits, int bitPosition)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Int32 | bits | number of bits in value to write |
| System.Int32 | bitPosition | where to write bits |

##### Returns
| Type | Description |
| ---- | ---- |
| System.UInt64 |  |

#### Skip(Int32)



##### Declaration

```cs
public void Skip(int bits)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Int32 | bits |  |


#### MoveBitPosition(Int32)


Moves the internal bit position
For most usecases it is safer to use 
WARNING: When reading from earlier position make sure to move position back to end of buffer after reading



##### Declaration

```cs
public void MoveBitPosition(int newPosition)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Int32 | newPosition |  |


#### PadAndCopy&lt;T&gt;(out T)



   Moves position to nearest byte then copies struct from that position




##### Declaration

```cs
public void PadAndCopy<T>(out T value)
    where T : struct
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| T | value |  |


#### ReadBytes(Byte[], Int32, Int32)



##### Declaration

```cs
public void ReadBytes(byte[] array, int offset, int length)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Byte[] | array |  |
| System.Int32 | offset |  |
| System.Int32 | length |  |


#### ReadSpanRaw(Span&lt;Byte&gt;)


Moves position to nearest byte then copies bytes from that position into the span



##### Declaration

```cs
public void ReadSpanRaw(Span<byte> span)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Span&lt;System.Byte&gt; | span |  |


#### ReadSpanRaw(Int32)



##### Declaration

```cs
public ReadOnlySpan<byte> ReadSpanRaw(int count)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Int32 | count |  |

##### Returns
| Type | Description |
| ---- | ---- |
| ReadOnlySpan&lt;System.Byte&gt; |  |

#### ReadBytesSegment(Int32)



##### Declaration

```cs
public ArraySegment<byte> ReadBytesSegment(int count)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Int32 | count |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.ArraySegment&lt;System.Byte&gt; |  |

