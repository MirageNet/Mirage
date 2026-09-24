---
id: BitHelper
title: BitHelper
---

# Class BitHelper



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
public static class BitHelper
```

### Methods
#### BitCount(Single, Single)


Gets the number of bits need for precision in range negative to positive max

WARNING: these methods are not fast, dont use in hotpath




##### Declaration

```cs
public static int BitCount(float max, float precision)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Single | max |  |
| System.Single | precision | lowest precision required, bit count will round up so real precision might be higher |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Int32 |  |

#### BitCount(Single, Single, Boolean)


Gets the number of bits need for precision in range max
If signed then range is negative max to positive max, If unsigned then 0 to max

WARNING: these methods are not fast, dont use in hotpath




##### Declaration

```cs
public static int BitCount(float max, float precision, bool signed)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Single | max |  |
| System.Single | precision | lowest precision required, bit count will round up so real precision might be higher |
| System.Boolean | signed |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Int32 |  |

#### BitCount(UInt64)


Gets the number of bits need for max

WARNING: these methods are not fast, dont use in hotpath




##### Declaration

```cs
public static int BitCount(ulong max)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.UInt64 | max |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Int32 |  |

