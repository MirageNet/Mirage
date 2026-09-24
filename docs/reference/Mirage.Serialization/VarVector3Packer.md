---
id: VarVector3Packer
title: VarVector3Packer
---

# Class VarVector3Packer


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
public sealed class VarVector3Packer
```

### Constructors

#### VarVector3Packer(Vector3, Int32)



##### Declaration

```cs
public VarVector3Packer(Vector3 precision, int blocksize)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Vector3 | precision |  |
| System.Int32 | blocksize |  |
### Methods
#### Pack(NetworkWriter, Vector3)



##### Declaration

```cs
public void Pack(NetworkWriter writer, Vector3 position)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| Vector3 | position |  |


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

