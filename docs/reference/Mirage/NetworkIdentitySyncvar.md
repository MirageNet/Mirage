---
id: NetworkIdentitySyncvar
title: NetworkIdentitySyncvar
---

# Struct NetworkIdentitySyncvar


backing struct for a NetworkIdentity when used as a syncvar
the weaver will replace the syncvar with this struct.




##### Syntax

```cs
public struct NetworkIdentitySyncvar : IEquatable<NetworkIdentitySyncvar>
```


### Properties

#### Value

##### Declaration

```cs
public NetworkIdentity Value { get; set; }
```
### Methods
#### Equals(NetworkIdentitySyncvar)



##### Declaration

```cs
public bool Equals(NetworkIdentitySyncvar other)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.NetworkIdentitySyncvar | other |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean |  |

