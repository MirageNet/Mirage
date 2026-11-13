---
id: DefaultSpawnValuesHandler
title: DefaultSpawnValuesHandler
---

# Class DefaultSpawnValuesHandler



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
public sealed class DefaultSpawnValuesHandler : ISpawnValuesHandler
```


### Fields

#### Instance

##### Declaration

```cs
public static readonly DefaultSpawnValuesHandler Instance
```
### Methods
#### CreateSpawnValues(NetworkIdentity)



##### Declaration

```cs
public SpawnValues CreateSpawnValues(NetworkIdentity identity)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.NetworkIdentity | identity |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.SpawnValues |  |

#### GetPrefabPosition(NetworkIdentity, SpawnValues)



##### Declaration

```cs
public (Vector3 pos, Quaternion rot) GetPrefabPosition(NetworkIdentity prefab, SpawnValues values)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.NetworkIdentity | prefab |  |
| Mirage.SpawnValues | values |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.ValueTuple{Vector3,Quaternion} |  |

#### ApplySpawnValues(NetworkIdentity, SpawnValues)



##### Declaration

```cs
public void ApplySpawnValues(NetworkIdentity identity, SpawnValues values)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.NetworkIdentity | identity |  |
| Mirage.SpawnValues | values |  |


