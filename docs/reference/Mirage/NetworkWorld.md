---
id: NetworkWorld
title: NetworkWorld
---

# Class NetworkWorld


Holds collection of spawned network objects
This class works on both server and client



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
public class NetworkWorld : IObjectLocator
```

### Constructors

#### NetworkWorld()



##### Declaration

```cs
public NetworkWorld()
```

### Properties

#### Time

Time kept in this world


##### Declaration

```cs
public NetworkTime Time { get; }
```
#### SpawnedIdentities

##### Declaration

```cs
public IReadOnlyCollection<NetworkIdentity> SpawnedIdentities { get; }
```
### Methods
#### GetSortedIdentities()


A list of spawned identities, sorted by netId.
This list is cached and will only be re-sorted if identities have changed



##### Declaration

```cs
public IReadOnlyList<NetworkIdentity> GetSortedIdentities()
```

##### Returns
| Type | Description |
| ---- | ---- |
| System.Collections.Generic.IReadOnlyList&lt;Mirage.NetworkIdentity&gt; |  |

#### TryGetIdentity(UInt32, out NetworkIdentity)



##### Declaration

```cs
public bool TryGetIdentity(uint netId, out NetworkIdentity identity)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.UInt32 | netId |  |
| Mirage.NetworkIdentity | identity |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean |  |

#### RemoveDestroyedObjects()



##### Declaration

```cs
public void RemoveDestroyedObjects()
```


