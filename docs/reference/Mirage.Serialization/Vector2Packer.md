---
id: Vector2Packer
title: Vector2Packer
---

# Class Vector2Packer



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
public sealed class Vector2Packer
```

### Constructors

#### Vector2Packer(Single, Single, Int32, Int32)



##### Declaration

```cs
public Vector2Packer(float xMax, float yMax, int xBitCount, int yBitCount)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Single | xMax |  |
| System.Single | yMax |  |
| System.Int32 | xBitCount |  |
| System.Int32 | yBitCount |  |

#### Vector2Packer(Single, Single, Single, Single)



##### Declaration

```cs
public Vector2Packer(float xMax, float yMax, float xPrecision, float yPrecision)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Single | xMax |  |
| System.Single | yMax |  |
| System.Single | xPrecision |  |
| System.Single | yPrecision |  |

#### Vector2Packer(Vector2, Vector2)



##### Declaration

```cs
public Vector2Packer(Vector2 max, Vector2 precision)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Vector2 | max |  |
| Vector2 | precision |  |
### Methods
#### Pack(NetworkWriter, Vector2)



##### Declaration

```cs
public void Pack(NetworkWriter writer, Vector2 value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| Vector2 | value |  |


#### Unpack(NetworkReader)



##### Declaration

```cs
public Vector2 Unpack(NetworkReader reader)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Vector2 |  |

