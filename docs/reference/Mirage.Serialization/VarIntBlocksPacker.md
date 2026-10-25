---
id: VarIntBlocksPacker
title: VarIntBlocksPacker
---

# Class VarIntBlocksPacker



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
public static class VarIntBlocksPacker
```

### Methods
#### Pack(NetworkWriter, UInt64, Int32)



##### Declaration

```cs
public static void Pack(NetworkWriter writer, ulong value, int blockSize)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| System.UInt64 | value |  |
| System.Int32 | blockSize |  |


#### Unpack(NetworkReader, Int32)



##### Declaration

```cs
public static ulong Unpack(NetworkReader reader, int blockSize)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |
| System.Int32 | blockSize |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.UInt64 |  |

