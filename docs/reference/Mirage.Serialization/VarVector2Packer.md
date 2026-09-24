---
id: VarVector2Packer
title: VarVector2Packer
---

# Class VarVector2Packer


Packs a vector3 using  and 



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
public sealed class VarVector2Packer
```

### Constructors

#### VarVector2Packer(Vector2, Int32)



##### Declaration

```cs
public VarVector2Packer(Vector2 precision, int blocksize)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Vector2 | precision |  |
| System.Int32 | blocksize |  |
### Methods
#### Pack(NetworkWriter, Vector2)



##### Declaration

```cs
public void Pack(NetworkWriter writer, Vector2 position)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| Vector2 | position |  |


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

