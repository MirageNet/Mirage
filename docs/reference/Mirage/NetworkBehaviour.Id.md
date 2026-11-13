---
id: NetworkBehaviour.Id
title: NetworkBehaviour.Id
---

# Struct NetworkBehaviour.Id




##### Syntax

```cs
public struct Id : IEquatable<NetworkBehaviour.Id>
```

### Constructors

#### Id(UInt32, Int32)



##### Declaration

```cs
public Id(uint netId, int componentIndex)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.UInt32 | netId |  |
| System.Int32 | componentIndex |  |

#### Id(NetworkBehaviour)



##### Declaration

```cs
public Id(NetworkBehaviour behaviour)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.NetworkBehaviour | behaviour |  |

### Fields

#### NetId

##### Declaration

```cs
public readonly uint NetId
```
#### ComponentIndex

##### Declaration

```cs
public readonly int ComponentIndex
```
### Methods
#### GetHashCode()



##### Declaration

```cs
public override int GetHashCode()
```

##### Returns
| Type | Description |
| ---- | ---- |
| System.Int32 |  |

#### Equals(NetworkBehaviour.Id)



##### Declaration

```cs
public bool Equals(NetworkBehaviour.Id other)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.NetworkBehaviour.Id | other |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean |  |

#### Equals(Object)



##### Declaration

```cs
public override bool Equals(object obj)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Object | obj |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean |  |

