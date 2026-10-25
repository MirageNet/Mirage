---
id: NetworkBehaviorSyncvar
title: NetworkBehaviorSyncvar
---

# Struct NetworkBehaviorSyncvar


backing struct for a NetworkIdentity when used as a syncvar
the weaver will replace the syncvar with this struct.




##### Syntax

```cs
public struct NetworkBehaviorSyncvar : IEquatable<NetworkBehaviorSyncvar>
```

### Constructors

#### NetworkBehaviorSyncvar(NetworkBehaviour)



##### Declaration

```cs
public NetworkBehaviorSyncvar(NetworkBehaviour behaviour)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.NetworkBehaviour | behaviour |  |

### Properties

#### Value

##### Declaration

```cs
public NetworkBehaviour Value { get; set; }
```
### Methods
#### GetAs&lt;T&gt;()


returns Value cast as T



##### Declaration

```cs
public T GetAs<T>()
    where T : NetworkBehaviour
```

##### Returns
| Type | Description |
| ---- | ---- |
| T |  |

#### Equals(NetworkBehaviorSyncvar)



##### Declaration

```cs
public bool Equals(NetworkBehaviorSyncvar other)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.NetworkBehaviorSyncvar | other |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean |  |

