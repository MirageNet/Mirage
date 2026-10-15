---
id: MaxLengthAttribute
title: MaxLengthAttribute
---

# Class MaxLengthAttribute


Restricts the serialization and deserialization size of strings, collections (arrays, lists), 
or any custom type that has read/write overloads accepting an integer limit.
This will use the Write/Read with length functions and will work on any type that has writers/readers for those.



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
public class MaxLengthAttribute : Attribute, _Attribute
```

### Constructors

#### MaxLengthAttribute(Int32)



##### Declaration

```cs
public MaxLengthAttribute(int maxLength)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Int32 | maxLength |  |

### Fields

#### maxLength

##### Declaration

```cs
public readonly int maxLength
```
