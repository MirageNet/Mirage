---
id: RemoteCallCollection
title: RemoteCallCollection
---

# Class RemoteCallCollection



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
public class RemoteCallCollection
```

### Constructors

#### RemoteCallCollection(NetworkBehaviour[])



##### Declaration

```cs
public RemoteCallCollection(NetworkBehaviour[] behaviours)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.NetworkBehaviour[] | behaviours |  |

### Fields

#### Empty

##### Declaration

```cs
public static readonly RemoteCallCollection Empty
```
#### IndexOffset

This is set by NetworkIdentity when we register each NetworkBehaviour so that they can pass their own index in


##### Declaration

```cs
public readonly int[] IndexOffset
```
#### RemoteCalls

##### Declaration

```cs
public readonly RemoteCall[] RemoteCalls
```
### Methods
#### Register(Int32, String, Boolean, RpcInvokeType, NetworkBehaviour, RpcDelegate, RpcRateLimitConfig)



##### Declaration

```cs
public void Register(int relativeIndex, string name, bool cmdRequireAuthority, RpcInvokeType invokerType, NetworkBehaviour behaviour, RpcDelegate func, RpcRateLimitConfig rateLimit)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Int32 | relativeIndex |  |
| System.String | name |  |
| System.Boolean | cmdRequireAuthority |  |
| Mirage.RemoteCalls.RpcInvokeType | invokerType |  |
| Mirage.NetworkBehaviour | behaviour |  |
| Mirage.RemoteCalls.RpcDelegate | func |  |
| Mirage.RemoteCalls.RpcRateLimitConfig | rateLimit |  |


#### RegisterRequest&lt;T&gt;(Int32, String, Boolean, RpcInvokeType, NetworkBehaviour, RequestDelegate&lt;T&gt;, RpcRateLimitConfig)



##### Declaration

```cs
public void RegisterRequest<T>(int relativeIndex, string name, bool cmdRequireAuthority, RpcInvokeType invokerType, NetworkBehaviour behaviour, RequestDelegate<T> func, RpcRateLimitConfig rateLimit)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Int32 | relativeIndex |  |
| System.String | name |  |
| System.Boolean | cmdRequireAuthority |  |
| Mirage.RemoteCalls.RpcInvokeType | invokerType |  |
| Mirage.NetworkBehaviour | behaviour |  |
| Mirage.RemoteCalls.RequestDelegate&lt;T&gt; | func |  |
| Mirage.RemoteCalls.RpcRateLimitConfig | rateLimit |  |


#### GetIndexOffset(NetworkBehaviour)



##### Declaration

```cs
public int GetIndexOffset(NetworkBehaviour behaviour)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.NetworkBehaviour | behaviour |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Int32 |  |

#### GetRelative(NetworkBehaviour, Int32)



##### Declaration

```cs
public RemoteCall GetRelative(NetworkBehaviour behaviour, int relativeIndex)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.NetworkBehaviour | behaviour |  |
| System.Int32 | relativeIndex |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.RemoteCalls.RemoteCall |  |

#### GetAbsolute(Int32)



##### Declaration

```cs
public RemoteCall GetAbsolute(int absoluteIndex)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Int32 | absoluteIndex |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.RemoteCalls.RemoteCall |  |

