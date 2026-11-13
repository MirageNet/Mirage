---
id: FloatPackAttribute
title: FloatPackAttribute
---

# Class FloatPackAttribute


Packs a float field, clamped from -max to +max, with
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
public class FloatPackAttribute : Attribute, _Attribute
```

### Constructors

#### FloatPackAttribute(Single, Single)



##### Declaration

```cs
public FloatPackAttribute(float max, float precision)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Single | max | Max value of the float |
| System.Single | precision | Smallest possible value of the field. Real precision will be calculated using bitcount but will always be lower than this parameter |

#### FloatPackAttribute(Single, Int32)



##### Declaration

```cs
public FloatPackAttribute(float max, int bitCount)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Single | max | Max value of the float |
| System.Int32 | bitCount | number of bits to pack the field into |
