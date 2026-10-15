---
id: SpawnHandler
title: SpawnHandler
---

# Class SpawnHandler



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
public class SpawnHandler
```

### Constructors

#### SpawnHandler(NetworkIdentity)



##### Declaration

```cs
public SpawnHandler(NetworkIdentity prefab)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.NetworkIdentity | prefab |  |

#### SpawnHandler(SpawnHandlerDelegate, UnSpawnDelegate)



##### Declaration

```cs
public SpawnHandler(SpawnHandlerDelegate spawnHandler, UnSpawnDelegate unspawnHandler)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.SpawnHandlerDelegate | spawnHandler |  |
| Mirage.UnSpawnDelegate | unspawnHandler |  |

#### SpawnHandler(NetworkIdentity, SpawnHandlerDelegate, UnSpawnDelegate)



##### Declaration

```cs
public SpawnHandler(NetworkIdentity prefab, SpawnHandlerDelegate spawnHandler, UnSpawnDelegate unspawnHandler)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.NetworkIdentity | prefab |  |
| Mirage.SpawnHandlerDelegate | spawnHandler |  |
| Mirage.UnSpawnDelegate | unspawnHandler |  |

#### SpawnHandler(SpawnHandlerAsyncDelegate, UnSpawnDelegate)



##### Declaration

```cs
public SpawnHandler(SpawnHandlerAsyncDelegate spawnHandlerAsync, UnSpawnDelegate unspawnHandler)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.SpawnHandlerAsyncDelegate | spawnHandlerAsync |  |
| Mirage.UnSpawnDelegate | unspawnHandler |  |

#### SpawnHandler(NetworkIdentity, SpawnHandlerAsyncDelegate, UnSpawnDelegate)



##### Declaration

```cs
public SpawnHandler(NetworkIdentity prefab, SpawnHandlerAsyncDelegate spawnHandlerAsync, UnSpawnDelegate unspawnHandler)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.NetworkIdentity | prefab |  |
| Mirage.SpawnHandlerAsyncDelegate | spawnHandlerAsync |  |
| Mirage.UnSpawnDelegate | unspawnHandler |  |

### Fields

#### Prefab

##### Declaration

```cs
public readonly NetworkIdentity Prefab
```
#### Handler

##### Declaration

```cs
public readonly SpawnHandlerDelegate Handler
```
#### HandlerAsync

##### Declaration

```cs
public readonly SpawnHandlerAsyncDelegate HandlerAsync
```

### Properties

#### UnspawnHandler

##### Declaration

```cs
public UnSpawnDelegate UnspawnHandler { get; }
```
### Methods
#### AddUnspawnHandler(UnSpawnDelegate)



##### Declaration

```cs
public void AddUnspawnHandler(UnSpawnDelegate unspawnHandler)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.UnSpawnDelegate | unspawnHandler |  |


#### IsAsyncSpawn()



##### Declaration

```cs
public bool IsAsyncSpawn()
```

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean |  |

