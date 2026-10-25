---
id: ByteUtils
title: ByteUtils
---

# Class ByteUtils



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
public static class ByteUtils
```

### Methods
#### WriteByte(Span&lt;Byte&gt;, ref Int32, Byte)



##### Declaration

```cs
public static void WriteByte(Span<byte> span, ref int offset, byte value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Span&lt;System.Byte&gt; | span |  |
| System.Int32 | offset |  |
| System.Byte | value |  |


#### ReadByte(ReadOnlySpan&lt;Byte&gt;, ref Int32)



##### Declaration

```cs
public static byte ReadByte(ReadOnlySpan<byte> span, ref int offset)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| ReadOnlySpan&lt;System.Byte&gt; | span |  |
| System.Int32 | offset |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Byte |  |

#### WriteUShort(Span&lt;Byte&gt;, ref Int32, UInt16)



##### Declaration

```cs
public static void WriteUShort(Span<byte> span, ref int offset, ushort value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Span&lt;System.Byte&gt; | span |  |
| System.Int32 | offset |  |
| System.UInt16 | value |  |


#### ReadUShort(ReadOnlySpan&lt;Byte&gt;, ref Int32)



##### Declaration

```cs
public static ushort ReadUShort(ReadOnlySpan<byte> span, ref int offset)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| ReadOnlySpan&lt;System.Byte&gt; | span |  |
| System.Int32 | offset |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.UInt16 |  |

#### WriteUInt(Span&lt;Byte&gt;, ref Int32, UInt32)



##### Declaration

```cs
public static void WriteUInt(Span<byte> buffer, ref int offset, uint value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Span&lt;System.Byte&gt; | buffer |  |
| System.Int32 | offset |  |
| System.UInt32 | value |  |


#### ReadUInt(ReadOnlySpan&lt;Byte&gt;, ref Int32)



##### Declaration

```cs
public static uint ReadUInt(ReadOnlySpan<byte> span, ref int offset)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| ReadOnlySpan&lt;System.Byte&gt; | span |  |
| System.Int32 | offset |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.UInt32 |  |

#### WriteULong(Span&lt;Byte&gt;, ref Int32, UInt64)



##### Declaration

```cs
public static void WriteULong(Span<byte> span, ref int offset, ulong value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Span&lt;System.Byte&gt; | span |  |
| System.Int32 | offset |  |
| System.UInt64 | value |  |


#### ReadULong(ReadOnlySpan&lt;Byte&gt;, ref Int32)



##### Declaration

```cs
public static ulong ReadULong(ReadOnlySpan<byte> span, ref int offset)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| ReadOnlySpan&lt;System.Byte&gt; | span |  |
| System.Int32 | offset |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.UInt64 |  |

