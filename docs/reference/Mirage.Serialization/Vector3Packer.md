---
id: Vector3Packer
title: Vector3Packer
---

# Class Vector3Packer



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
public sealed class Vector3Packer
```

### Constructors

#### Vector3Packer(Single, Single, Single, Int32, Int32, Int32)



##### Declaration

```cs
public Vector3Packer(float xMax, float yMax, float zMax, int xBitCount, int yBitCount, int zBitCount)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Single | xMax |  |
| System.Single | yMax |  |
| System.Single | zMax |  |
| System.Int32 | xBitCount |  |
| System.Int32 | yBitCount |  |
| System.Int32 | zBitCount |  |

#### Vector3Packer(Single, Single, Single, Single, Single, Single)



##### Declaration

```cs
public Vector3Packer(float xMax, float yMax, float zMax, float xPrecision, float yPrecision, float zPrecision)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Single | xMax |  |
| System.Single | yMax |  |
| System.Single | zMax |  |
| System.Single | xPrecision |  |
| System.Single | yPrecision |  |
| System.Single | zPrecision |  |

#### Vector3Packer(Vector3, Vector3)



##### Declaration

```cs
public Vector3Packer(Vector3 max, Vector3 precision)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Vector3 | max |  |
| Vector3 | precision |  |
### Methods
#### Pack(NetworkWriter, Vector3)



##### Declaration

```cs
public void Pack(NetworkWriter writer, Vector3 value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| Vector3 | value |  |


#### Unpack(NetworkReader)



##### Declaration

```cs
public Vector3 Unpack(NetworkReader reader)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Vector3 |  |

