---
id: PackedExtensions
title: PackedExtensions
---

# Class PackedExtensions



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
public static class PackedExtensions
```

### Methods
#### WritePackedInt32(NetworkWriter, Int32)



##### Declaration

```cs
public static void WritePackedInt32(this NetworkWriter writer, int i)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| System.Int32 | i |  |


#### WritePackedUInt32(NetworkWriter, UInt32)



##### Declaration

```cs
public static void WritePackedUInt32(this NetworkWriter writer, uint value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| System.UInt32 | value |  |


#### WritePackedInt64(NetworkWriter, Int64)



##### Declaration

```cs
public static void WritePackedInt64(this NetworkWriter writer, long i)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| System.Int64 | i |  |


#### WritePackedUInt64(NetworkWriter, UInt64)



##### Declaration

```cs
public static void WritePackedUInt64(this NetworkWriter writer, ulong value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| System.UInt64 | value |  |


#### ReadPackedInt32(NetworkReader)



##### Declaration

```cs
public static int ReadPackedInt32(this NetworkReader reader)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Int32 |  |

#### ReadPackedUInt32(NetworkReader)



##### Declaration

```cs
public static uint ReadPackedUInt32(this NetworkReader reader)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.UInt32 |  |

#### ReadPackedInt64(NetworkReader)



##### Declaration

```cs
public static long ReadPackedInt64(this NetworkReader reader)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Int64 |  |

#### ReadPackedUInt64(NetworkReader)



##### Declaration

```cs
public static ulong ReadPackedUInt64(this NetworkReader reader)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.UInt64 |  |

