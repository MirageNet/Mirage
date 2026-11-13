---
id: AnglePacker
title: AnglePacker
---

# Class AnglePacker



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
public sealed class AnglePacker
```

### Constructors

#### AnglePacker(Single)



##### Declaration

```cs
public AnglePacker(float lowestPrecision)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Single | lowestPrecision | lowest precision, actual precision will be caculated from number of bits used |
### Methods
#### Pack(Single)



##### Declaration

```cs
public uint Pack(float value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Single | value |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.UInt32 |  |

#### Pack(NetworkWriter, Single)



##### Declaration

```cs
public void Pack(NetworkWriter writer, float value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| System.Single | value |  |


#### Unpack(UInt32)



##### Declaration

```cs
public float Unpack(uint value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.UInt32 | value |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Single |  |

#### Unpack(NetworkReader)



##### Declaration

```cs
public float Unpack(NetworkReader reader)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Single |  |

