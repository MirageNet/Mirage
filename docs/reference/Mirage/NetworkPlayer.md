---
id: NetworkPlayer
title: NetworkPlayer
---

# Class NetworkPlayer


A High level network connection. This is used for connections from client-to-server and for connection from server-to-client.



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
public sealed class NetworkPlayer : INetworkPlayer, IMessageSender, IVisibilityTracker, IObjectOwner, ISceneLoader
```

### Constructors

#### NetworkPlayer(IConnection, Boolean, NetworkServer, Nullable&lt;RateLimitBucket.RefillConfig&gt;)


Creates a new NetworkPlayer



##### Declaration

```cs
public NetworkPlayer(IConnection connection, bool isHost, NetworkServer server, RateLimitBucket.RefillConfig? errorRateLimit)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.SocketLayer.IConnection | connection | Transport level connection for this player |
| System.Boolean | isHost | True if this player is the host player |
| Mirage.NetworkServer | server | The server that created this player, can be null for client side players |
| System.Nullable&lt;Mirage.SocketLayer.RateLimitBucket.RefillConfig&gt; | errorRateLimit |  |

### Fields

#### RpcRateLimit

##### Declaration

```cs
public Dictionary<RpcId, RateLimitBucket> RpcRateLimit
```

### Properties

#### IsHost

##### Declaration

```cs
public bool IsHost { get; }
```
#### ErrorFlags
Any flags set from catching errors

##### Declaration

```cs
public PlayerErrorFlags ErrorFlags { get; }
```
#### ErrorRateLimit
Error rate limiting, will invoke disconnect player (or call  if set) when limit is reached

##### Declaration

```cs
public RateLimitBucket ErrorRateLimit { get; }
```
#### Authentication

Authentication information for this NetworkPlayer


##### Declaration

```cs
public PlayerAuthentication Authentication { get; }
```
#### IsAuthenticated

Helper methods to check if Authentication is set


##### Declaration

```cs
public bool IsAuthenticated { get; }
```
#### SceneIsReady

Flag that tells us if the scene has fully loaded in for player.

A client that is ready is sent spawned objects by the server and updates to the state of spawned objects. A client that is not ready is not sent spawned objects.


Starts as true, when a client connects it is assumed that it is already in a ready scene.


It will be set to not ready when a scene load is started.
If you are controlling scene loading manually, you need to set this property to true or false before and after loading a scene.
This is normally done using  and 


On the client, this property is used to keep track of if the local scene is loading or ready.
On the server, it is used to track if the player&apos;s scene is loading or ready.
When server loads a new scene for everyone, it will normally set this property to false for all players.



##### Declaration

```cs
public bool SceneIsReady { get; set; }
```
#### HasCharacter

Checks if this player has a 


##### Declaration

```cs
public bool HasCharacter { get; }
```
#### Connection

##### Declaration

```cs
public IConnection Connection { get; }
```
#### ConnectionHandle

The IP address / URL / FQDN associated with the connection.
Can be useful for a game master to do IP Bans etc.

Best used to get concrete Endpoint type based on the  being used



##### Declaration

```cs
public IConnectionHandle ConnectionHandle { get; }
```
#### IsConnecting
Connect called on client, but server has not replied yet

##### Declaration

```cs
public bool IsConnecting { get; }
```
#### IsConnected
Server and Client are connected and can send messages

##### Declaration

```cs
public bool IsConnected { get; }
```
#### VisList

List of all networkIdentity that this player can see
Only valid on server


##### Declaration

```cs
public IReadOnlyCollection<NetworkIdentity> VisList { get; }
```
#### OwnedObjects

A list of the NetworkIdentity objects owned by this connection. This list is read-only.
Only valid on server
This includes the player&apos;s character
This list can be used to validate messages from clients, to ensure that clients are only trying to control objects that they own.
Objects in the list will also have their  field set to this NetworkPlayer


##### Declaration

```cs
public IReadOnlyCollection<NetworkIdentity> OwnedObjects { get; }
```
#### Identity

The NetworkIdentity for this connection.


##### Declaration

```cs
public NetworkIdentity Identity { get; set; }
```
### Methods
#### SetAuthentication(PlayerAuthentication, Boolean)



##### Declaration

```cs
public void SetAuthentication(PlayerAuthentication authentication, bool allowReplace)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Authentication.PlayerAuthentication | authentication |  |
| System.Boolean | allowReplace |  |


#### Disconnect()


Disconnects the player.
A disconnected player can not send messages



##### Declaration

```cs
public void Disconnect()
```


#### Disconnect(DisconnectReason)


Disconnects the player.
A disconnected player can not send messages



##### Declaration

```cs
public void Disconnect(DisconnectReason reason)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.SocketLayer.DisconnectReason | reason |  |


#### MarkAsDisconnected()


Marks player as disconnected, used when the disconnect call is from peer
A disconnected player can not send messages



##### Declaration

```cs
public void MarkAsDisconnected()
```


#### Send&lt;T&gt;(T, Channel)


This sends a network message to the connection.



##### Declaration

```cs
public void Send<T>(T message, Channel channelId = Channel.Reliable)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| T | message |  |
| Mirage.Channel | channelId | The transport layer channel to send on. |


#### Send(ArraySegment&lt;Byte&gt;, Channel)


Sends a block of data

This is a low-level method to send a raw bytes.
Only use this method if you have manually included the message id and serialized the payload.
Otherwise, receivers will not know how to handle it.
It is recommended to use  instead.



##### Declaration

```cs
public void Send(ArraySegment<byte> segment, Channel channelId = Channel.Reliable)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.ArraySegment&lt;System.Byte&gt; | segment |  |
| Mirage.Channel | channelId |  |


#### Send&lt;T&gt;(T, INotifyCallBack)


This sends a network message to the connection.



##### Declaration

```cs
public void Send<T>(T message, INotifyCallBack callBacks)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| T | message |  |
| Mirage.SocketLayer.INotifyCallBack | callBacks |  |


#### ToString()



##### Declaration

```cs
public override string ToString()
```

##### Returns
| Type | Description |
| ---- | ---- |
| System.String |  |

#### AddToVisList(NetworkIdentity)



##### Declaration

```cs
public void AddToVisList(NetworkIdentity identity)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.NetworkIdentity | identity |  |


#### RemoveFromVisList(NetworkIdentity)



##### Declaration

```cs
public void RemoveFromVisList(NetworkIdentity identity)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.NetworkIdentity | identity |  |


#### ContainsInVisList(NetworkIdentity)


Checks if player can see NetworkIdentity



##### Declaration

```cs
public bool ContainsInVisList(NetworkIdentity identity)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.NetworkIdentity | identity |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean |  |

#### RemoveAllVisibleObjects()


Removes all objects that this player can see
This is called when loading a new scene



##### Declaration

```cs
public void RemoveAllVisibleObjects()
```


#### AddOwnedObject(NetworkIdentity)



##### Declaration

```cs
public void AddOwnedObject(NetworkIdentity identity)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.NetworkIdentity | identity |  |


#### RemoveOwnedObject(NetworkIdentity)



##### Declaration

```cs
public void RemoveOwnedObject(NetworkIdentity identity)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.NetworkIdentity | identity |  |


#### RemoveAllOwnedObject(Boolean)



##### Declaration

```cs
public void RemoveAllOwnedObject(bool sendAuthorityChangeEvent)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Boolean | sendAuthorityChangeEvent |  |


#### DestroyOwnedObjects()


Destroy all objects owned by this player
NOTE: only destroyed objects that are currently spawned



##### Declaration

```cs
public void DestroyOwnedObjects()
```


#### SetError(Int32, PlayerErrorFlags)


Call this when player causes an error



##### Declaration

```cs
public void SetError(int cost, PlayerErrorFlags flags)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Int32 | cost | how bad or costly is the error. higher cost means player will trigger limit faster |
| Mirage.PlayerErrorFlags | flags | optional flag for error type |


#### SetErrorAndDisconnect(PlayerErrorFlags)


Call this when player causes an error, will set cost to be above maxTokens to ensure that limit is cheated to trigger disconnect.
If  is null will call  instead



##### Declaration

```cs
public void SetErrorAndDisconnect(PlayerErrorFlags flags)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.PlayerErrorFlags | flags | optional flag for error type |


#### ResetErrorFlag()

Call to reset error flags


##### Declaration

```cs
public void ResetErrorFlag()
```


#### CheckRateLimit(RemoteCall)


Checks and enforces rate limiting for an RPC.
Returns true if the RPC is allowed to execute.
If false, the RPC should be ignored.



##### Declaration

```cs
public bool CheckRateLimit(RemoteCall remoteCall)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.RemoteCalls.RemoteCall | remoteCall |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean |  |

