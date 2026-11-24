---
id: INetworkPlayer
title: INetworkPlayer
---

# Interface INetworkPlayer


An object owned by a player that can: send/receive messages, have network visibility, be an object owner, authenticated permissions, and load scenes.
May be from the server to client or from client to server




##### Syntax

```cs
public interface INetworkPlayer : IMessageSender, IVisibilityTracker, IObjectOwner, ISceneLoader
```


### Properties

#### Connection

Connection object managed by 

This is used to send messages and handle any reliability state for the underlying connection



##### Declaration

```cs
IConnection Connection { get; }
```
#### ConnectionHandle

The low-level handle returned by .
Can be used to find out more information about the low-level transport used or to get the Address of the connection.
Cast this to the handle type for the transport you are using.
example: if (ConnectionHandle is UdpConnectionHandle udpHandle) and then get the address via udpHandle.Endpoint


##### Declaration

```cs
IConnectionHandle ConnectionHandle { get; }
```
#### IsConnecting
Connect called on client, but server has not replied yet

##### Declaration

```cs
bool IsConnecting { get; }
```
#### IsConnected
Server and Client are connected and can send messages

##### Declaration

```cs
bool IsConnected { get; }
```
#### Authentication

##### Declaration

```cs
PlayerAuthentication Authentication { get; }
```
#### IsAuthenticated

##### Declaration

```cs
bool IsAuthenticated { get; }
```
#### ErrorRateLimit
Error rate limiting, will invoke disconnect player (or call  if set) when limit is reached

##### Declaration

```cs
RateLimitBucket ErrorRateLimit { get; }
```
#### ErrorFlags
Any flags set from catching errors

##### Declaration

```cs
PlayerErrorFlags ErrorFlags { get; }
```
#### IsHost
True if this Player is the local player on the server or client

##### Declaration

```cs
bool IsHost { get; }
```
### Methods
#### SetAuthentication(PlayerAuthentication, Boolean)



##### Declaration

```cs
void SetAuthentication(PlayerAuthentication authentication, bool allowReplace = false)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Authentication.PlayerAuthentication | authentication |  |
| System.Boolean | allowReplace |  |


#### SetError(Int32, PlayerErrorFlags)


Call this when player causes an error



##### Declaration

```cs
void SetError(int cost, PlayerErrorFlags flags)
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
void SetErrorAndDisconnect(PlayerErrorFlags flags)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.PlayerErrorFlags | flags | optional flag for error type |


#### ResetErrorFlag()

Call to reset error flags


##### Declaration

```cs
void ResetErrorFlag()
```


#### Disconnect()



##### Declaration

```cs
void Disconnect()
```


#### MarkAsDisconnected()



##### Declaration

```cs
void MarkAsDisconnected()
```


#### CheckRateLimit(RemoteCall)


Checks and enforces rate limiting for an RPC.
Returns true if the RPC is allowed to execute.
If false, the RPC should be ignored.



##### Declaration

```cs
bool CheckRateLimit(RemoteCall remoteCall)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.RemoteCalls.RemoteCall | remoteCall |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean |  |

