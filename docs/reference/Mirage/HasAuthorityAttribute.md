---
id: HasAuthorityAttribute
title: HasAuthorityAttribute
---

# Class HasAuthorityAttribute


Prevents players without authority from running this method.
Can only be used inside a NetworkBehaviour



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
[AttributeUsage(AttributeTargets.Method)]
public class HasAuthorityAttribute : Attribute, _Attribute
```


### Fields

#### error

If true,  when the method is called from a client, it throws an error
If false, no error is thrown, but the method won&apos;t execute
useful for unity built in methods such as Await, Update, Start, etc.


##### Declaration

```cs
public bool error
```
