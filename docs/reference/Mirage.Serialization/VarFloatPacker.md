---
id: VarFloatPacker
title: VarFloatPacker
---

# Class VarFloatPacker


Packs a float using  and 



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
public sealed class VarFloatPacker
```

### Constructors

#### VarFloatPacker(Single, Int32)



##### Declaration

```cs
public VarFloatPacker(float precision, int blockSize)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Single | precision |  |
| System.Int32 | blockSize |  |
### Methods
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

