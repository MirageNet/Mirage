---
id: RemoteCallCollectionCache
title: RemoteCallCollectionCache
---

# Class RemoteCallCollectionCache


Caches immutable RPC collection layouts to avoid re-allocating delegate wrappers and array buffers per spawned identity



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
public static class RemoteCallCollectionCache
```


### Properties

#### Empty

##### Declaration

```cs
public static RemoteCallCollection Empty { get; }
```
### Methods
#### GetOrCreate(NetworkBehaviour[])



##### Declaration

```cs
public static RemoteCallCollection GetOrCreate(NetworkBehaviour[] behaviours)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.NetworkBehaviour[] | behaviours |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.RemoteCalls.RemoteCallCollection |  |

#### Clear()



##### Declaration

```cs
public static void Clear()
```


