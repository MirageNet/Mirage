---
id: BitCountFromRangeAttribute
title: BitCountFromRangeAttribute
---

# Class BitCountFromRangeAttribute


Calculates bitcount from then given min/max values and then packs using 
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
public class BitCountFromRangeAttribute : Attribute, _Attribute
```

### Constructors

#### BitCountFromRangeAttribute(Int32, Int32)



##### Declaration

```cs
public BitCountFromRangeAttribute(int min, int max)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Int32 | min | minimum possible int value |
| System.Int32 | max | minimum possible max value |
