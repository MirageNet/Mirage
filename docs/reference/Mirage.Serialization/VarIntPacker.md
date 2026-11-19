---
id: VarIntPacker
title: VarIntPacker
---

# Class VarIntPacker



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
public sealed class VarIntPacker
```

### Constructors

#### VarIntPacker(UInt64, UInt64)



##### Declaration

```cs
public VarIntPacker(ulong smallValue, ulong mediumValue)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.UInt64 | smallValue |  |
| System.UInt64 | mediumValue |  |

#### VarIntPacker(UInt64, UInt64, UInt64, Boolean)



##### Declaration

```cs
public VarIntPacker(ulong smallValue, ulong mediumValue, ulong largeValue, bool throwIfOverLarge = true)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.UInt64 | smallValue |  |
| System.UInt64 | mediumValue |  |
| System.UInt64 | largeValue |  |
| System.Boolean | throwIfOverLarge |  |
### Methods
#### FromBitCount(Int32, Int32)



##### Declaration

```cs
public static VarIntPacker FromBitCount(int smallBits, int mediumBits)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Int32 | smallBits |  |
| System.Int32 | mediumBits |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.Serialization.VarIntPacker |  |

#### FromBitCount(Int32, Int32, Int32, Boolean)



##### Declaration

```cs
public static VarIntPacker FromBitCount(int smallBits, int mediumBits, int largeBits, bool throwIfOverLarge = true)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Int32 | smallBits |  |
| System.Int32 | mediumBits |  |
| System.Int32 | largeBits |  |
| System.Boolean | throwIfOverLarge |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.Serialization.VarIntPacker |  |

#### PackUlong(NetworkWriter, UInt64)



##### Declaration

```cs
public void PackUlong(NetworkWriter writer, ulong value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| System.UInt64 | value |  |


#### PackUint(NetworkWriter, UInt32)



##### Declaration

```cs
public void PackUint(NetworkWriter writer, uint value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| System.UInt32 | value |  |


#### PackUshort(NetworkWriter, UInt16)



##### Declaration

```cs
public void PackUshort(NetworkWriter writer, ushort value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| System.UInt16 | value |  |


#### UnpackUlong(NetworkReader)



##### Declaration

```cs
public ulong UnpackUlong(NetworkReader reader)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.UInt64 |  |

#### UnpackUint(NetworkReader)



##### Declaration

```cs
public uint UnpackUint(NetworkReader reader)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.UInt32 |  |

#### UnpackUshort(NetworkReader)



##### Declaration

```cs
public ushort UnpackUshort(NetworkReader reader)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.UInt16 |  |

