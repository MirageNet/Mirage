---
id: SystemTypesExtensions
title: SystemTypesExtensions
---

# Class SystemTypesExtensions



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
public static class SystemTypesExtensions
```

### Methods
#### WriteByteExtension(NetworkWriter, Byte)



##### Declaration

```cs
public static void WriteByteExtension(this NetworkWriter writer, byte value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| System.Byte | value |  |


#### WriteSByteExtension(NetworkWriter, SByte)



##### Declaration

```cs
public static void WriteSByteExtension(this NetworkWriter writer, sbyte value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| System.SByte | value |  |


#### WriteChar(NetworkWriter, Char)



##### Declaration

```cs
public static void WriteChar(this NetworkWriter writer, char value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| System.Char | value |  |


#### WriteBooleanExtension(NetworkWriter, Boolean)



##### Declaration

```cs
public static void WriteBooleanExtension(this NetworkWriter writer, bool value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| System.Boolean | value |  |


#### WriteUInt16Extension(NetworkWriter, UInt16)



##### Declaration

```cs
public static void WriteUInt16Extension(this NetworkWriter writer, ushort value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| System.UInt16 | value |  |


#### WriteInt16Extension(NetworkWriter, Int16)



##### Declaration

```cs
public static void WriteInt16Extension(this NetworkWriter writer, short value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| System.Int16 | value |  |


#### WriteSingleConverter(NetworkWriter, Single)



##### Declaration

```cs
public static void WriteSingleConverter(this NetworkWriter writer, float value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| System.Single | value |  |


#### WriteDoubleConverter(NetworkWriter, Double)



##### Declaration

```cs
public static void WriteDoubleConverter(this NetworkWriter writer, double value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| System.Double | value |  |


#### WriteDecimalConverter(NetworkWriter, Decimal)



##### Declaration

```cs
public static void WriteDecimalConverter(this NetworkWriter writer, decimal value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| System.Decimal | value |  |


#### WriteGuid(NetworkWriter, Guid)



##### Declaration

```cs
public static void WriteGuid(this NetworkWriter writer, Guid value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| System.Guid | value |  |


#### WriteNullable&lt;T&gt;(NetworkWriter, Nullable&lt;T&gt;)



##### Declaration

```cs
[WeaverSerializeCollection]
public static void WriteNullable<T>(this NetworkWriter writer, T? nullable)
    where T : struct
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| System.Nullable&lt;T&gt; | nullable |  |


#### ReadByteExtension(NetworkReader)



##### Declaration

```cs
public static byte ReadByteExtension(this NetworkReader reader)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Byte |  |

#### ReadSByteExtension(NetworkReader)



##### Declaration

```cs
public static sbyte ReadSByteExtension(this NetworkReader reader)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.SByte |  |

#### ReadChar(NetworkReader)



##### Declaration

```cs
public static char ReadChar(this NetworkReader reader)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Char |  |

#### ReadBooleanExtension(NetworkReader)



##### Declaration

```cs
public static bool ReadBooleanExtension(this NetworkReader reader)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean |  |

#### ReadInt16Extension(NetworkReader)



##### Declaration

```cs
public static short ReadInt16Extension(this NetworkReader reader)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Int16 |  |

#### ReadUInt16Extension(NetworkReader)



##### Declaration

```cs
public static ushort ReadUInt16Extension(this NetworkReader reader)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.UInt16 |  |

#### ReadSingleConverter(NetworkReader)



##### Declaration

```cs
public static float ReadSingleConverter(this NetworkReader reader)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Single |  |

#### ReadDoubleConverter(NetworkReader)



##### Declaration

```cs
public static double ReadDoubleConverter(this NetworkReader reader)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Double |  |

#### ReadDecimalConverter(NetworkReader)



##### Declaration

```cs
public static decimal ReadDecimalConverter(this NetworkReader reader)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Decimal |  |

#### ReadGuid(NetworkReader)



##### Declaration

```cs
public static Guid ReadGuid(this NetworkReader reader)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Guid |  |

#### ReadNullable&lt;T&gt;(NetworkReader)



##### Declaration

```cs
[WeaverSerializeCollection]
public static T? ReadNullable<T>(this NetworkReader reader)
    where T : struct
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Nullable&lt;T&gt; |  |

