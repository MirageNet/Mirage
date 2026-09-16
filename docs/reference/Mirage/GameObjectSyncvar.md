---
id: GameObjectSyncvar
title: GameObjectSyncvar
---

# Struct GameObjectSyncvar


backing struct for a NetworkIdentity when used as a syncvar
the weaver will replace the syncvar with this struct.




##### Syntax

```cs
public struct GameObjectSyncvar : IEquatable<GameObjectSyncvar>
```


### Properties

#### Value

##### Declaration

```cs
public GameObject Value { get; set; }
```
### Methods
#### Equals(GameObjectSyncvar)



##### Declaration

```cs
public bool Equals(GameObjectSyncvar other)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.GameObjectSyncvar | other |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean |  |

