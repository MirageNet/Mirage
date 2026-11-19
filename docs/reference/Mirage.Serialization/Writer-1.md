---
id: Writer-1
title: Writer<T>
---

# Class Writer&lt;T&gt;


a class that holds writers for the different types
Note that c# creates a different static variable for each
type
This will be populated by the weaver



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
public static class Writer<T>
```

##### Type Parameters
| Name | Description |
| ---- | ---- |
| T |  |


### Properties

#### Write

##### Declaration

```cs
public static Action<NetworkWriter, T> Write { set; }
```
#### WriteWithLength

##### Declaration

```cs
public static Action<NetworkWriter, T, int> WriteWithLength { set; }
```
