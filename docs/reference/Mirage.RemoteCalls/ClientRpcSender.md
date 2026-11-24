---
id: ClientRpcSender
title: ClientRpcSender
---

# Class ClientRpcSender



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
public static class ClientRpcSender
```

### Methods
#### Send(NetworkBehaviour, Int32, NetworkWriter, Channel, Boolean)



##### Declaration

```cs
public static void Send(NetworkBehaviour behaviour, int relativeIndex, NetworkWriter writer, Channel channelId, bool excludeOwner)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.NetworkBehaviour | behaviour |  |
| System.Int32 | relativeIndex |  |
| Mirage.Serialization.NetworkWriter | writer |  |
| Mirage.Channel | channelId |  |
| System.Boolean | excludeOwner |  |


#### SendTarget(NetworkBehaviour, Int32, NetworkWriter, Channel, INetworkPlayer)



##### Declaration

```cs
public static void SendTarget(NetworkBehaviour behaviour, int relativeIndex, NetworkWriter writer, Channel channelId, INetworkPlayer player)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.NetworkBehaviour | behaviour |  |
| System.Int32 | relativeIndex |  |
| Mirage.Serialization.NetworkWriter | writer |  |
| Mirage.Channel | channelId |  |
| Mirage.INetworkPlayer | player |  |


#### SendTargetWithReturn&lt;T&gt;(NetworkBehaviour, Int32, NetworkWriter, INetworkPlayer)



##### Declaration

```cs
public static UniTask<T> SendTargetWithReturn<T>(NetworkBehaviour behaviour, int relativeIndex, NetworkWriter writer, INetworkPlayer player)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.NetworkBehaviour | behaviour |  |
| System.Int32 | relativeIndex |  |
| Mirage.Serialization.NetworkWriter | writer |  |
| Mirage.INetworkPlayer | player |  |

##### Returns
| Type | Description |
| ---- | ---- |
| UniTask&lt;T&gt; |  |

#### ShouldInvokeLocally(NetworkBehaviour, RpcTarget, INetworkPlayer, Boolean, Boolean)


Used by weaver to check if ClientRPC should be invoked locally in host mode



##### Declaration

```cs
public static bool ShouldInvokeLocally(NetworkBehaviour behaviour, RpcTarget target, INetworkPlayer player, bool excludeOwner, bool includeServer)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.NetworkBehaviour | behaviour |  |
| Mirage.RpcTarget | target |  |
| Mirage.INetworkPlayer | player | player used for RpcTarget.Player |
| System.Boolean | excludeOwner |  |
| System.Boolean | includeServer | if true, invoke on server even if not host |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean |  |

#### IsLocalPlayerObserver(NetworkBehaviour, Boolean)


Checks if host player can see the object
Weaver uses this to check if RPC should be invoked locally



##### Declaration

```cs
public static bool IsLocalPlayerObserver(NetworkBehaviour behaviour, bool excludeOwner)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.NetworkBehaviour | behaviour |  |
| System.Boolean | excludeOwner |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean |  |

#### IsLocalPlayerTarget(NetworkBehaviour, INetworkPlayer)


Checks if host player is the target player
Weaver uses this to check if RPC should be invoked locally



##### Declaration

```cs
public static bool IsLocalPlayerTarget(NetworkBehaviour behaviour, INetworkPlayer target)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.NetworkBehaviour | behaviour |  |
| Mirage.INetworkPlayer | target |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean |  |

#### GetLocalInvokePlayer(NetworkBehaviour)


Gets the INetworkPlayer arg to pass in when ClientRpc is invoked locally
Weaver uses this instead of Client.Player so that it is safe to call on server only



##### Declaration

```cs
public static INetworkPlayer GetLocalInvokePlayer(NetworkBehaviour behaviour)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.NetworkBehaviour | behaviour |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.INetworkPlayer | Player for connection to the server, or null when there is no local client |

