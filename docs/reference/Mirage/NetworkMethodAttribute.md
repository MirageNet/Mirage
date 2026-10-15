---
id: NetworkMethodAttribute
title: NetworkMethodAttribute
---

# Class NetworkMethodAttribute


Prevents this method from running unless the NetworkFlags match the current state
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
public class NetworkMethodAttribute : Attribute, _Attribute
```

### Constructors

#### NetworkMethodAttribute(NetworkFlags)



##### Declaration

```cs
public NetworkMethodAttribute(NetworkFlags flags)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.NetworkFlags | flags |  |

### Fields

#### error

If true, if called incorrectly method will throw.
If false, no error is thrown, but the method won&apos;t execute.

useful for unity built in methods such as Await, Update, Start, etc.



##### Declaration

```cs
public bool error
```
