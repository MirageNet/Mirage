---
id: CompressedExtensions
title: CompressedExtensions
---

# Class CompressedExtensions



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
public static class CompressedExtensions
```

### Methods
#### WriteQuaternion(NetworkWriter, Quaternion)


Packs Quaternion using 



##### Declaration

```cs
public static void WriteQuaternion(this NetworkWriter writer, Quaternion rotation)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| Quaternion | rotation |  |


#### ReadQuaternion(NetworkReader)


Unpacks Quaternion using 



##### Declaration

```cs
public static Quaternion ReadQuaternion(this NetworkReader reader)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Quaternion |  |

