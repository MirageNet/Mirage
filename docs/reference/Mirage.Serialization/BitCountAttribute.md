---
id: BitCountAttribute
title: BitCountAttribute
---

# Class BitCountAttribute


Tells weaver how many bits to sue for field
Only works with integer fields (byte, int, ulong, enums etc)

NOTE: bits are truncated when using this, so signed values will lose their sign. Use  as well if value might be negative

Also See: Bit Packing Documentation



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
public class BitCountAttribute : Attribute, _Attribute
```

### Constructors

#### BitCountAttribute(Int32)



##### Declaration

```cs
public BitCountAttribute(int bitCount)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Int32 | bitCount | Value should be between 1 and 64 |
