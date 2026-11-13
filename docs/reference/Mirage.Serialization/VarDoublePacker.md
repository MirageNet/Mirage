---
id: VarDoublePacker
title: VarDoublePacker
---

# Class VarDoublePacker


Packs a double using  and 



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
public sealed class VarDoublePacker
```

### Constructors

#### VarDoublePacker(Double, Int32)



##### Declaration

```cs
public VarDoublePacker(double precision, int blockSize)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Double | precision |  |
| System.Int32 | blockSize |  |
### Methods
#### Pack(NetworkWriter, Double)



##### Declaration

```cs
public void Pack(NetworkWriter writer, double value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| System.Double | value |  |


#### Unpack(NetworkReader)



##### Declaration

```cs
public double Unpack(NetworkReader reader)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Double |  |

