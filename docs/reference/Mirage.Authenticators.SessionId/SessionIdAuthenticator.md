---
id: SessionIdAuthenticator
title: SessionIdAuthenticator
---

# Class SessionIdAuthenticator



<div class="inheritance">

##### Inheritance

<div class="level" style={{"--data-index": 0}}>
System.Object
</div>
<div class="level" style={{"--data-index": 1}}>
Mirage.Authentication.NetworkAuthenticator
</div>
<div class="level" style={{"--data-index": 2}}>
Mirage.Authentication.NetworkAuthenticator&lt;Mirage.Authenticators.SessionId.SessionKeyMessage&gt;
</div>
</div>

##### Inherited Members

<details>
<summary>Show</summary>

Mirage.Authentication.NetworkAuthenticator&lt;Mirage.Authenticators.SessionId.SessionKeyMessage&gt;.AuthenticateAsync(Mirage.INetworkPlayer, Mirage.Authenticators.SessionId.SessionKeyMessage, System.Threading.CancellationToken)


Mirage.Authentication.NetworkAuthenticator&lt;Mirage.Authenticators.SessionId.SessionKeyMessage&gt;.SendAuthentication(Mirage.NetworkClient, Mirage.Authenticators.SessionId.SessionKeyMessage)


Mirage.Authentication.NetworkAuthenticator.AuthenticatorName

</details>

##### Syntax

```cs
public class SessionIdAuthenticator : NetworkAuthenticator<SessionKeyMessage>, INetworkAuthenticator
```


### Fields

#### NO_KEY_ERROR

##### Declaration

```cs
public const string NO_KEY_ERROR = "Empty key from client"
```
#### NOT_FOUND_ERROR

##### Declaration

```cs
public const string NOT_FOUND_ERROR = "No session found"
```
#### SessionIDLength

##### Declaration

```cs
public int SessionIDLength
```
#### TimeoutMinutes

##### Declaration

```cs
public int TimeoutMinutes
```
#### ClientIdStore

Set on client to save key somewhere. For example as a cookie on webgl

By default it is just stored in memory



##### Declaration

```cs
public ISessionIdStore ClientIdStore
```
### Methods
#### Authenticate(INetworkPlayer, SessionKeyMessage)



##### Declaration

```cs
protected override AuthenticationResult Authenticate(INetworkPlayer player, SessionKeyMessage message)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.INetworkPlayer | player |  |
| Mirage.Authenticators.SessionId.SessionKeyMessage | message |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.Authentication.AuthenticationResult |  |

#### CreateOrRefreshSession(INetworkPlayer)



##### Declaration

```cs
public ArraySegment<byte> CreateOrRefreshSession(INetworkPlayer player)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.INetworkPlayer | player |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.ArraySegment&lt;System.Byte&gt; |  |

