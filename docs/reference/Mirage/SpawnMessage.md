---
id: SpawnMessage
title: SpawnMessage
---

# Struct SpawnMessage




##### Syntax

```cs
public struct SpawnMessage
```


### Fields

#### NetId

netId of new or existing object


##### Declaration

```cs
public uint NetId
```
#### IsLocalPlayer

Is the spawning object the local player. Sets ClientScene.localPlayer


##### Declaration

```cs
public bool IsLocalPlayer
```
#### IsOwner

Sets hasAuthority on the spawned object


##### Declaration

```cs
public bool IsOwner
```
#### SceneId

The id of the scene object to spawn


##### Declaration

```cs
public ulong? SceneId
```
#### PrefabHash

The id of the prefab to spawn
If sceneId != 0 then it is used instead of prefabHash


##### Declaration

```cs
public int? PrefabHash
```
#### SpawnValues

Spawn values to set after spawning object, values based on 


##### Declaration

```cs
public SpawnValues SpawnValues
```
#### Payload

The serialized component data
ArraySegment to avoid unnecessary allocations


##### Declaration

```cs
public ArraySegment<byte> Payload
```
### Methods
#### ToString()



##### Declaration

```cs
public override string ToString()
```

##### Returns
| Type | Description |
| ---- | ---- |
| System.String |  |

