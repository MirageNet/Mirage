---
id: AutoPool-1
title: AutoPool<T>
---

# Class AutoPool&lt;T&gt;


Pool class that will create a Disposable wrapper around T so it can be used with any class automatically without additional setup



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
public static class AutoPool<T>
    where T : class, new()
```

##### Type Parameters
| Name | Description |
| ---- | ---- |
| T |  |


### Fields

#### Pool

Default pool instance, safe to use on main thread


##### Declaration

```cs
public static Pool<AutoPool<T>.Wrapper> Pool
```
### Methods
#### Take()



##### Declaration

```cs
public static AutoPool<T>.Wrapper Take()
```

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.AutoPool.Wrapper&lt;&gt; |  |

