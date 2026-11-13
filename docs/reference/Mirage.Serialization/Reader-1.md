---
id: Reader-1
title: Reader<T>
---

# Class Reader&lt;T&gt;


a class that holds readers for the different types
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
public static class Reader<T>
```

##### Type Parameters
| Name | Description |
| ---- | ---- |
| T |  |


### Properties

#### Read

##### Declaration

```cs
public static Func<NetworkReader, T> Read { set; }
```
#### ReadWithLength

##### Declaration

```cs
public static Func<NetworkReader, int, T> ReadWithLength { set; }
```
