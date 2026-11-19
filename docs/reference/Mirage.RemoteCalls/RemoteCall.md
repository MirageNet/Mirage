---
id: RemoteCall
title: RemoteCall
---

# Class RemoteCall


Used for invoking a RPC methods



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
public class RemoteCall
```

### Constructors

#### RemoteCall(Type, Int32, Int32, RpcInvokeType, RpcDelegate, Boolean, String, RpcRateLimitConfig)



##### Declaration

```cs
public RemoteCall(Type declaringType, int componentIndex, int indexInType, RpcInvokeType invokeType, RpcDelegate function, bool requireAuthority, string name, RpcRateLimitConfig rateLimit)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Type | declaringType |  |
| System.Int32 | componentIndex |  |
| System.Int32 | indexInType |  |
| Mirage.RemoteCalls.RpcInvokeType | invokeType |  |
| Mirage.RemoteCalls.RpcDelegate | function |  |
| System.Boolean | requireAuthority |  |
| System.String | name |  |
| Mirage.RemoteCalls.RpcRateLimitConfig | rateLimit |  |

### Fields

#### DeclaringType

Type that rpc was declared in


##### Declaration

```cs
public readonly Type DeclaringType
```
#### ComponentIndex

Component index within NetworkIdentity.NetworkBehaviours


##### Declaration

```cs
public readonly int ComponentIndex
```
#### RelativeIndex

Relative index of this RPC within its declaring component


##### Declaration

```cs
public readonly int RelativeIndex
```
#### InvokeType

Server rpc or client rpc


##### Declaration

```cs
public readonly RpcInvokeType InvokeType
```
#### Function

Function to be invoked when receiving message


##### Declaration

```cs
public readonly RpcDelegate Function
```
#### RequireAuthority

Used by ServerRpc


##### Declaration

```cs
public readonly bool RequireAuthority
```
#### Name

User friendly name


##### Declaration

```cs
public readonly string Name
```
#### RateLimit

Rate limit configuration for this RPC


##### Declaration

```cs
public readonly RpcRateLimitConfig RateLimit
```
#### RpcId

Stable Id for an RPC inside a type


##### Declaration

```cs
public readonly RpcId RpcId
```
#### InvokeMarker

##### Declaration

```cs
public readonly ProfilerMarker InvokeMarker
```
### Methods
#### ToString()


User friendly name used for debug/error messages



##### Declaration

```cs
public override string ToString()
```

##### Returns
| Type | Description |
| ---- | ---- |
| System.String |  |

