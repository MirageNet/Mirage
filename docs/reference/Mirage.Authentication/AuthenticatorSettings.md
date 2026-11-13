---
id: AuthenticatorSettings
title: AuthenticatorSettings
---

# Class AuthenticatorSettings



<div class="inheritance">

##### Inheritance

<div class="level" style={{"--data-index": 0}}>
System.Object
</div>
</div>

##### Syntax

```cs
public sealed class AuthenticatorSettings : MonoBehaviour
```


### Fields

#### TimeoutSeconds

##### Declaration

```cs
public int TimeoutSeconds
```
#### RequireHostToAuthenticate

##### Declaration

```cs
public bool RequireHostToAuthenticate
```
#### Authenticators

##### Declaration

```cs
public List<NetworkAuthenticator> Authenticators
```
### Methods
#### Setup(NetworkServer)



##### Declaration

```cs
public void Setup(NetworkServer server)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.NetworkServer | server |  |


#### ServerAuthenticate(INetworkPlayer)



##### Declaration

```cs
public UniTask<AuthenticationResult> ServerAuthenticate(INetworkPlayer player)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.INetworkPlayer | player |  |

##### Returns
| Type | Description |
| ---- | ---- |
| UniTask&lt;Mirage.Authentication.AuthenticationResult&gt; |  |

#### GetCancellationToken(INetworkPlayer)



##### Declaration

```cs
public CancellationToken GetCancellationToken(INetworkPlayer player)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.INetworkPlayer | player |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Threading.CancellationToken |  |

