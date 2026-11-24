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
| T | Type of item to pool. IList and IDictionary are cleared on return, other collections should be taken via <xref href="Mirage.AutoPool" data-throw-if-not-resolved="false"></xref> helpers |


### Fields

#### Pool

Default pool instance, safe to use on main thread


##### Declaration

```cs
public static Pool<AutoPool<T>.Wrapper> Pool
```
#### ClearAction

Called when an item is returned to the pool, for types that are not  or .
Shared by all users of this T. Set by the collection helpers in  on their first call,
so items taken directly via  before that are not cleared


##### Declaration

```cs
public static Action<T> ClearAction
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

#### Take(out T)



##### Declaration

```cs
public static AutoPool<T>.Wrapper Take(out T item)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| T | item |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.AutoPool.Wrapper&lt;&gt; |  |

