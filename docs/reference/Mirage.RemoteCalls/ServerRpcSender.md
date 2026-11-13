---
id: ServerRpcSender
title: ServerRpcSender
---

# Class ServerRpcSender


Methods used by weaver to send RPCs



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
public static class ServerRpcSender
```

### Methods
#### Send(NetworkBehaviour, Int32, NetworkWriter, Channel, Boolean)



##### Declaration

```cs
public static void Send(NetworkBehaviour behaviour, int relativeIndex, NetworkWriter writer, Channel channelId, bool requireAuthority)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.NetworkBehaviour | behaviour |  |
| System.Int32 | relativeIndex |  |
| Mirage.Serialization.NetworkWriter | writer |  |
| Mirage.Channel | channelId |  |
| System.Boolean | requireAuthority |  |


#### SendWithReturn&lt;T&gt;(NetworkBehaviour, Int32, NetworkWriter, Boolean)



##### Declaration

```cs
public static UniTask<T> SendWithReturn<T>(NetworkBehaviour behaviour, int relativeIndex, NetworkWriter writer, bool requireAuthority)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.NetworkBehaviour | behaviour |  |
| System.Int32 | relativeIndex |  |
| Mirage.Serialization.NetworkWriter | writer |  |
| System.Boolean | requireAuthority |  |

##### Returns
| Type | Description |
| ---- | ---- |
| UniTask&lt;T&gt; |  |

#### ShouldInvokeLocally(NetworkBehaviour, Boolean, Boolean)


Used by weaver to check if ClientRPC should be invoked locally in host mode



##### Declaration

```cs
public static bool ShouldInvokeLocally(NetworkBehaviour behaviour, bool requireAuthority, bool allowServerToCall)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.NetworkBehaviour | behaviour |  |
| System.Boolean | requireAuthority |  |
| System.Boolean | allowServerToCall |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean |  |

