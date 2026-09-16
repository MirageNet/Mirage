---
id: ClientObjectManager
title: ClientObjectManager
---

# Class ClientObjectManager



<div class="inheritance">

##### Inheritance

<div class="level" style={{"--data-index": 0}}>
System.Object
</div>
</div>

##### Syntax

```cs
public class ClientObjectManager : MonoBehaviour
```


### Fields

#### spawnPrefabs

List of prefabs that will be registered with the spawning system.
For each of these prefabs, ClientManager.RegisterPrefab() will be automatically invoke.


##### Declaration

```cs
public List<NetworkIdentity> spawnPrefabs
```
#### NetworkPrefabs

A scriptable object that holds all the prefabs that will be registered with the spawning system.
For each of these prefabs, ClientManager.RegisterPrefab() will be automatically invoked.


##### Declaration

```cs
public NetworkPrefabs NetworkPrefabs
```
#### spawnableObjects

This is dictionary of the disabled NetworkIdentity objects in the scene that could be spawned by messages from the server.
The key to the dictionary is the NetworkIdentity sceneId.


##### Declaration

```cs
public readonly Dictionary<ulong, NetworkIdentity> spawnableObjects
```

### Properties

#### Client

##### Declaration

```cs
public NetworkClient Client { get; }
```
#### SceneObjectFilter

A filter to only include certain scene objects when spawning.
Used by  when finding scene objects.
If the filter is null, all valid scene objects will be included.


##### Declaration

```cs
public Func<NetworkIdentity, bool> SceneObjectFilter { get; set; }
```
### Methods
#### PrepareToSpawnSceneObjects()


Call this after loading/unloading a scene in the client after connection to register the spawnable objects



##### Declaration

```cs
public void PrepareToSpawnSceneObjects()
```


#### RegisterPrefabs(IEnumerable&lt;NetworkIdentity&gt;, Boolean)


Calls  on each object in the prefabs collection



##### Declaration

```cs
public void RegisterPrefabs(IEnumerable<NetworkIdentity> prefabs, bool skipExisting)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Collections.Generic.IEnumerable&lt;Mirage.NetworkIdentity&gt; | prefabs |  |
| System.Boolean | skipExisting | Dont call <xref href="Mirage.ClientObjectManager.RegisterPrefab(Mirage.NetworkIdentity%2cSystem.Int32)" data-throw-if-not-resolved="false"></xref> for prefab&apos;s who&apos;s hash is already in the list of handlers. This can happen if custom handler is added for a prefab in the insepctor list |


#### GetSpawnHandler(Int32)


Find the registered or dynamic handler for prefabHash
Useful for debuggers



##### Declaration

```cs
public SpawnHandler GetSpawnHandler(int prefabHash)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Int32 | prefabHash | asset id of the prefab |

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.SpawnHandler | true if prefab was registered |

#### RegisterPrefab(NetworkIdentity, Int32)


Registers a prefab with the spawning system.

When a NetworkIdentity object is spawned on the server with ServerObjectManager.Spawn(),
the server will send a spawn message to the client with the PrefabHash.
the client then finds the prefab registered with RegisterPrefab() to instantiate the client object.

The ClientObjectManager has a list of spawnable prefabs, it uses this function to register them.
The set of current spawnable object is available in the  dictionary.



##### Declaration

```cs
public void RegisterPrefab(NetworkIdentity identity, int newPrefabHash)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.NetworkIdentity | identity | A Prefab that will be spawned. |
| System.Int32 | newPrefabHash | A hash to be assigned to this prefab. This allows a dynamically created game object to be registered for an already known asset Id. |


#### RegisterPrefab(NetworkIdentity)


Registers a prefab with the spawning system.

When a NetworkIdentity object is spawned on the server with ServerObjectManager.Spawn(),
the server will send a spawn message to the client with the PrefabHash.
the client then finds the prefab registered with RegisterPrefab() to instantiate the client object.

The ClientObjectManager has a list of spawnable prefabs, it uses this function to register them.
The set of current spawnable object is available in the  dictionary.



##### Declaration

```cs
public void RegisterPrefab(NetworkIdentity identity)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.NetworkIdentity | identity | A Prefab that will be spawned. |


#### RegisterUnspawnHandler(NetworkIdentity, UnSpawnDelegate)


Registers an unspawn handler for a prefab
Should be called after RegisterPrefab



##### Declaration

```cs
public void RegisterUnspawnHandler(NetworkIdentity identity, UnSpawnDelegate unspawnHandler)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.NetworkIdentity | identity | Prefab to add handler for |
| Mirage.UnSpawnDelegate | unspawnHandler | A method to use as a custom un-spawnhandler on clients. |


#### UnregisterPrefab(NetworkIdentity)


Removes a registered spawn prefab that was setup with RegisterPrefab.



##### Declaration

```cs
public void UnregisterPrefab(NetworkIdentity identity)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.NetworkIdentity | identity | The prefab to be removed from registration. |


#### RegisterSpawnHandler(NetworkIdentity, SpawnHandlerDelegate, UnSpawnDelegate)


Registers custom handlers for a prefab with the spawning system.

When a NetworkIdentity object is spawned on the server with ServerObjectManager.Spawn(),
the server will send a spawn message to the client with the PrefabHash.
the client then finds the prefab registered with RegisterPrefab() to instantiate the client object.

The ClientObjectManager has a list of spawnable prefabs, it uses this function to register them.
The set of current spawnable object is available in the  dictionary.



##### Declaration

```cs
public void RegisterSpawnHandler(NetworkIdentity identity, SpawnHandlerDelegate spawnHandler, UnSpawnDelegate unspawnHandler)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.NetworkIdentity | identity | A Prefab that will be spawned. |
| Mirage.SpawnHandlerDelegate | spawnHandler | A method to use as a custom spawnhandler on clients. |
| Mirage.UnSpawnDelegate | unspawnHandler | A method to use as a custom un-spawnhandler on clients. |


#### RegisterSpawnHandler(Int32, SpawnHandlerDelegate, UnSpawnDelegate)


This is an advanced spawning function that registers a custom prefabHash with the UNET spawning system.
This can be used to register custom spawning methods for an prefabHash - instead of the usual method of registering spawning methods for a prefab. This should be used when no prefab exists for the spawned objects - such as when they are constructed dynamically at runtime from configuration data.



##### Declaration

```cs
public void RegisterSpawnHandler(int prefabHash, SpawnHandlerDelegate spawnHandler, UnSpawnDelegate unspawnHandler)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Int32 | prefabHash |  |
| Mirage.SpawnHandlerDelegate | spawnHandler | A method to use as a custom spawnhandler on clients. |
| Mirage.UnSpawnDelegate | unspawnHandler | A method to use as a custom un-spawnhandler on clients. |


#### RegisterSpawnHandler(NetworkIdentity, SpawnHandlerAsyncDelegate, UnSpawnDelegate)



##### Declaration

```cs
public void RegisterSpawnHandler(NetworkIdentity identity, SpawnHandlerAsyncDelegate spawnHandler, UnSpawnDelegate unspawnHandler)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.NetworkIdentity | identity |  |
| Mirage.SpawnHandlerAsyncDelegate | spawnHandler |  |
| Mirage.UnSpawnDelegate | unspawnHandler |  |


#### RegisterSpawnHandler(Int32, SpawnHandlerAsyncDelegate, UnSpawnDelegate)



##### Declaration

```cs
public void RegisterSpawnHandler(int prefabHash, SpawnHandlerAsyncDelegate spawnHandler, UnSpawnDelegate unspawnHandler)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Int32 | prefabHash |  |
| Mirage.SpawnHandlerAsyncDelegate | spawnHandler |  |
| Mirage.UnSpawnDelegate | unspawnHandler |  |


#### UnregisterSpawnHandler(Int32)


Removes a registered spawn handler function that was registered with RegisterSpawnHandler().



##### Declaration

```cs
public void UnregisterSpawnHandler(int prefabHash)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Int32 | prefabHash | The prefabHash for the handler to be removed for. |


#### ClearSpawners()


This clears the registered spawn prefabs and spawn handler functions for this client.



##### Declaration

```cs
public void ClearSpawners()
```


#### RegisterDynamicSpawnHandler(DynamicSpawnHandlerDelegate)



##### Declaration

```cs
public void RegisterDynamicSpawnHandler(DynamicSpawnHandlerDelegate dynamicHandler)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.DynamicSpawnHandlerDelegate | dynamicHandler |  |


#### DestroyAllClientObjects()


Destroys all networked objects on the client.
This can be used to clean up when a network connection is closed.



##### Declaration

```cs
public void DestroyAllClientObjects()
```


