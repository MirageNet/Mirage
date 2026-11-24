---
id: VarIntBlocksAttribute
title: VarIntBlocksAttribute
---

# Class VarIntBlocksAttribute


Tells weaver the block size to use for packing int values
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
public class VarIntBlocksAttribute : Attribute, _Attribute
```

### Constructors

#### VarIntBlocksAttribute(Int32)


Bit size of each block
how many bits per size bits,
eg if size = 6 then values under 2^6 will be sent at 7 bits, values under 2^12 sent as 14 bits, etc



##### Declaration

```cs
public VarIntBlocksAttribute(int blockSize)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Int32 | blockSize | Value should be between 1 and 64 |
