---
id: IObjectLocator
title: IObjectLocator
---

# Interface IObjectLocator


An object that implements this interface can find objects by their net id
This is used by readers when trying to deserialize gameobjects




##### Syntax

```cs
public interface IObjectLocator
```

### Methods
#### TryGetIdentity(UInt32, out NetworkIdentity)


Finds a network identity by id



##### Declaration

```cs
bool TryGetIdentity(uint netId, out NetworkIdentity identity)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.UInt32 | netId | the id of the object to find |
| Mirage.NetworkIdentity | identity | The NetworkIdentity matching the netId or null if none is found |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean | true if identity is found and is not null |

