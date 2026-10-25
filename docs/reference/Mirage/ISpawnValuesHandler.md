---
id: ISpawnValuesHandler
title: ISpawnValuesHandler
---

# Interface ISpawnValuesHandler




##### Syntax

```cs
public interface ISpawnValuesHandler
```

### Methods
#### CreateSpawnValues(NetworkIdentity)


Server-side: Generates the  to be sent across the network for this identity.



##### Declaration

```cs
SpawnValues CreateSpawnValues(NetworkIdentity identity)
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


Client-side: Returns the position and rotation used when instantiating a prefab.



##### Declaration

```cs
(Vector3 pos, Quaternion rot) GetPrefabPosition(NetworkIdentity prefab, SpawnValues values)
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


Client-side: Applies received spawn values (position, rotation, scale, name, active) to the identity.



##### Declaration

```cs
void ApplySpawnValues(NetworkIdentity identity, SpawnValues values)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.NetworkIdentity | identity |  |
| Mirage.SpawnValues | values |  |


