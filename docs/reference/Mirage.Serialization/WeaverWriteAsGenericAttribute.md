---
id: WeaverWriteAsGenericAttribute
title: WeaverWriteAsGenericAttribute
---

# Class WeaverWriteAsGenericAttribute


Tells Weaver to serialize a type as generic instead of creating a custom functions.

Use this when you have created and assigned your own Read/Write functions 
or when you can&apos;t use extension methods for types and need to manually assign them.


This will cause Weaver to use the  and  generic functions instead of creating new ones.
You must set these functions manually when using this attribute.




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
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Interface | AttributeTargets.Struct)]
public sealed class WeaverWriteAsGenericAttribute : Attribute, _Attribute
```

