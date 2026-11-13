---
id: SyncPrefab
title: SyncPrefab
---

# Struct SyncPrefab




##### Syntax

```cs
public struct SyncPrefab
```

### Constructors

#### SyncPrefab(NetworkIdentity)



##### Declaration

```cs
public SyncPrefab(NetworkIdentity prefab)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.NetworkIdentity | prefab |  |

#### SyncPrefab(Int32)



##### Declaration

```cs
public SyncPrefab(int hash)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Int32 | hash |  |

### Fields

#### Prefab

##### Declaration

```cs
public NetworkIdentity Prefab
```
#### PrefabHash

##### Declaration

```cs
public int PrefabHash
```
### Methods
#### FindPrefab(ClientObjectManager)


Searches ClientObjectManager to find a prefab using its hash



##### Declaration

```cs
public NetworkIdentity FindPrefab(ClientObjectManager manager)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.ClientObjectManager | manager |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.NetworkIdentity |  |

#### FindPrefab(IEnumerable&lt;NetworkIdentity&gt;)


Searches ClientObjectManager to find a prefab using its hash



##### Declaration

```cs
public NetworkIdentity FindPrefab(IEnumerable<NetworkIdentity> collection)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Collections.Generic.IEnumerable&lt;Mirage.NetworkIdentity&gt; | collection |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.NetworkIdentity |  |

