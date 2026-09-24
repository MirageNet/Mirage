---
id: NetworkBehaviorSyncvar-1
title: NetworkBehaviorSyncvar<T>
---

# Struct NetworkBehaviorSyncvar&lt;T&gt;




##### Syntax

```cs
public struct NetworkBehaviorSyncvar<T> : IEquatable<NetworkBehaviorSyncvar<T>>, IEquatable<NetworkBehaviorSyncvar> where T : NetworkBehaviour
```

##### Type Parameters
| Name | Description |
| ---- | ---- |
| T |  |

### Constructors

#### NetworkBehaviorSyncvar(T)



##### Declaration

```cs
public NetworkBehaviorSyncvar(T behaviour)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| T | behaviour |  |

### Properties

#### Value

##### Declaration

```cs
public T Value { get; set; }
```
### Methods
#### Equals(NetworkBehaviorSyncvar&lt;T&gt;)



##### Declaration

```cs
public bool Equals(NetworkBehaviorSyncvar<T> other)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.NetworkBehaviorSyncvar&lt;T&gt; | other |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean |  |

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

