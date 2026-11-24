---
id: AutoPool
title: AutoPool
---

# Class AutoPool


Helpers for taking collections from  so that they are cleared when returned to the pool
List and Dictionary are cleared by Wrapper.Dispose via IList/IDictionary, other collections need  set by these helpers



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
public static class AutoPool
```

### Methods
#### TakeList&lt;T&gt;(out List&lt;T&gt;)



##### Declaration

```cs
public static AutoPool<List<T>>.Wrapper TakeList<T>(out List<T> item)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Collections.Generic.List&lt;T&gt; | item |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.AutoPool.Wrapper&lt;&gt; |  |

#### TakeDictionary&lt;TKey, TValue&gt;(out Dictionary&lt;TKey, TValue&gt;)



##### Declaration

```cs
public static AutoPool<Dictionary<TKey, TValue>>.Wrapper TakeDictionary<TKey, TValue>(out Dictionary<TKey, TValue> item)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Collections.Generic.Dictionary&lt;TKey, TValue&gt; | item |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.AutoPool.Wrapper&lt;&gt; |  |

#### TakeHashSet&lt;T&gt;(out HashSet&lt;T&gt;)



##### Declaration

```cs
public static AutoPool<HashSet<T>>.Wrapper TakeHashSet<T>(out HashSet<T> item)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Collections.Generic.HashSet&lt;T&gt; | item |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.AutoPool.Wrapper&lt;&gt; |  |

#### TakeQueue&lt;T&gt;(out Queue&lt;T&gt;)



##### Declaration

```cs
public static AutoPool<Queue<T>>.Wrapper TakeQueue<T>(out Queue<T> item)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Collections.Generic.Queue&lt;T&gt; | item |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.AutoPool.Wrapper&lt;&gt; |  |

#### TakeStack&lt;T&gt;(out Stack&lt;T&gt;)



##### Declaration

```cs
public static AutoPool<Stack<T>>.Wrapper TakeStack<T>(out Stack<T> item)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Collections.Generic.Stack&lt;T&gt; | item |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.AutoPool.Wrapper&lt;&gt; |  |

