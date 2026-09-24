---
id: Pool-1
title: Pool<T>
---

# Class Pool&lt;T&gt;


Holds a collection of  so they can be re-used without allocations



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
public class Pool<T>
    where T : class
```

##### Type Parameters
| Name | Description |
| ---- | ---- |
| T |  |

### Constructors

#### Pool(Pool&lt;T&gt;.CreateNewItemNoCount, Int32, Int32, ILogger)


Creates pool, that does not require Buffer size



##### Declaration

```cs
public Pool(Pool<T>.CreateNewItemNoCount createNew, int startPoolSize, int maxPoolSize, ILogger logger = null)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.SocketLayer.Pool.CreateNewItemNoCount&lt;&gt; | createNew |  |
| System.Int32 | startPoolSize | how many buffers to create at start |
| System.Int32 | maxPoolSize | max number of buffers in pool |
| ILogger | logger |  |

#### Pool(Pool&lt;T&gt;.CreateNewItem, Int32, Int32, Int32, ILogger)


Creates pool where buffer size will be passed to items when created them



##### Declaration

```cs
public Pool(Pool<T>.CreateNewItem createNew, int bufferSize, int startPoolSize, int maxPoolSize, ILogger logger = null)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.SocketLayer.Pool.CreateNewItem&lt;&gt; | createNew |  |
| System.Int32 | bufferSize | size of each buffer |
| System.Int32 | startPoolSize | how many buffers to create at start |
| System.Int32 | maxPoolSize | max number of buffers in pool |
| ILogger | logger |  |

### Properties

#### Metrics

##### Declaration

```cs
public PoolMetrics Metrics { get; }
```
### Methods
#### Configure(Int32, Int32)


sets max pool size and then creates writers up to new start size



##### Declaration

```cs
public void Configure(int startPoolSize, int maxPoolSize)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Int32 | startPoolSize |  |
| System.Int32 | maxPoolSize |  |


#### Take()



##### Declaration

```cs
public T Take()
```

##### Returns
| Type | Description |
| ---- | ---- |
| T |  |

#### Put(T)


Puts item back in pool, or leaves it for GC if pool is full.



##### Declaration

```cs
public bool Put(T buffer)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| T | buffer |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean | returns false if pool is full, in this case, any unmanaged objects should be cleared up |

