---
id: NetworkWriterPool
title: NetworkWriterPool
---

# Class NetworkWriterPool



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
public static class NetworkWriterPool
```


### Properties

#### BufferSize

Current Size of buffers, or null before Configure has been called


##### Declaration

```cs
public static int? BufferSize { get; }
```
### Methods
#### Configure(Int32, Int32, Int32)


Configures an exist pool or creates a new one
Does not create a new pool if bufferSize is less that current 



##### Declaration

```cs
public static void Configure(int bufferSize, int startPoolSize = 5, int maxPoolSize = 100)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Int32 | bufferSize | starting capacity of buffer |
| System.Int32 | startPoolSize |  |
| System.Int32 | maxPoolSize |  |


#### GetWriter()



##### Declaration

```cs
public static PooledNetworkWriter GetWriter()
```

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.Serialization.PooledNetworkWriter |  |

