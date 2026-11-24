---
id: VarIntAttribute
title: VarIntAttribute
---

# Class VarIntAttribute


Tells weaver the max range for small, medium and large values.
Allows small values to be sent using less bits
Only works with integer fields (byte, int, ulong, enums etc)



<div class="inheritance">

##### Inheritance

<div class="level" style={{"--data-index": 0}}>
System.Object
</div>
<div class="level" style={{"--data-index": 1}}>
System.Attribute
</div>
</div>

##### Inherited Members

<details>
<summary>Show</summary>
</details>

##### Syntax

```cs
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter)]
public class VarIntAttribute : Attribute, _Attribute
```

### Constructors

#### VarIntAttribute(UInt64, UInt64)



##### Declaration

```cs
public VarIntAttribute(ulong smallMax, ulong mediumMax)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.UInt64 | smallMax |  |
| System.UInt64 | mediumMax |  |

#### VarIntAttribute(UInt64, UInt64, UInt64, Boolean)



##### Declaration

```cs
public VarIntAttribute(ulong smallMax, ulong mediumMax, ulong largeMax, bool throwIfOverLarge = true)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.UInt64 | smallMax |  |
| System.UInt64 | mediumMax |  |
| System.UInt64 | largeMax |  |
| System.Boolean | throwIfOverLarge |  |
