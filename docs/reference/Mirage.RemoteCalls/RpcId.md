---
id: RpcId
title: RpcId
---

# Struct RpcId




##### Syntax

```cs
public struct RpcId : IEquatable<RpcId>
```

### Constructors

#### RpcId(Type, Int32)



##### Declaration

```cs
public RpcId(Type declaringType, int index)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Type | declaringType |  |
| System.Int32 | index |  |

### Fields

#### Hash

##### Declaration

```cs
public readonly int Hash
```
#### DeclaringType

##### Declaration

```cs
public readonly Type DeclaringType
```
#### DeclaringIndex

##### Declaration

```cs
public readonly int DeclaringIndex
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

#### Equals(RpcId)



##### Declaration

```cs
public bool Equals(RpcId other)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.RemoteCalls.RpcId | other |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean |  |

