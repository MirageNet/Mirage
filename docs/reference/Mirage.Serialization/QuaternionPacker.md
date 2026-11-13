---
id: QuaternionPacker
title: QuaternionPacker
---

# Class QuaternionPacker



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
public sealed class QuaternionPacker
```

### Constructors

#### QuaternionPacker(Int32)



##### Declaration

```cs
public QuaternionPacker(int quaternionBitLength = 10)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Int32 | quaternionBitLength | 10 per &quot;smallest 3&quot; is good enough for most people |

### Fields

#### Default9
Default packer using 9 bits per element, 29 bits total

##### Declaration

```cs
public static readonly QuaternionPacker Default9
```
#### Default10
Default packer using 10 bits per element, 32 bits total

##### Declaration

```cs
public static readonly QuaternionPacker Default10
```
### Methods
#### PackAsInt(Quaternion)



##### Declaration

```cs
public static uint PackAsInt(Quaternion value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Quaternion | value |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.UInt32 |  |

#### UnpackFromInt(UInt32)



##### Declaration

```cs
public static Quaternion UnpackFromInt(uint value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.UInt32 | value |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Quaternion |  |

#### Pack(NetworkWriter, Quaternion)



##### Declaration

```cs
public void Pack(NetworkWriter writer, Quaternion value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| Quaternion | value |  |


#### Pack(Quaternion)



##### Declaration

```cs
public ulong Pack(Quaternion value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Quaternion | value |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.UInt64 |  |

#### Unpack(NetworkReader)



##### Declaration

```cs
public Quaternion Unpack(NetworkReader reader)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Quaternion |  |

#### Unpack(UInt64)



##### Declaration

```cs
public Quaternion Unpack(ulong combine)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.UInt64 | combine |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Quaternion |  |

