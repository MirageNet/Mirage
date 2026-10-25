---
id: BitMask
title: BitMask
---

# Class BitMask



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
public static class BitMask
```

### Methods
#### Mask(Int32)


Creates mask for bits

(showing 32 bits for simplify, result is 64 bit)

Example bits = 4 => mask = 00000000_00000000_00000000_00001111

Example bits = 10 => mask = 00000000_00000000_00000011_11111111




##### Declaration

```cs
public static ulong Mask(int bits)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Int32 | bits |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.UInt64 |  |

#### OuterMask(Int32, Int32)


Creates Mask either side of start and end
Note this mask is only valid for start [0..63] and end [0..64]



##### Declaration

```cs
public static ulong OuterMask(int start, int end)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Int32 | start |  |
| System.Int32 | end |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.UInt64 |  |

