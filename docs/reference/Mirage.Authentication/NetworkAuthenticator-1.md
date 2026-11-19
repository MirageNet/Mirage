---
id: NetworkAuthenticator-1
title: NetworkAuthenticator<T>
---

# Class NetworkAuthenticator&lt;T&gt;



<div class="inheritance">

##### Inheritance

<div class="level" style={{"--data-index": 0}}>
System.Object
</div>
<div class="level" style={{"--data-index": 1}}>
Mirage.Authentication.NetworkAuthenticator
</div>
</div>

##### Inherited Members

<details>
<summary>Show</summary>

Mirage.Authentication.NetworkAuthenticator.AuthenticatorName

</details>

##### Syntax

```cs
public abstract class NetworkAuthenticator<T> : NetworkAuthenticator, INetworkAuthenticator
```

##### Type Parameters
| Name | Description |
| ---- | ---- |
| T |  |

### Methods
#### AuthenticateAsync(INetworkPlayer, T, CancellationToken)


Called on server to Authenticate a message from client

Use  OR . 
By default the async version just call the normal version.




##### Declaration

```cs
protected virtual UniTask<AuthenticationResult> AuthenticateAsync(INetworkPlayer player, T message, CancellationToken cancellationToken)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.INetworkPlayer | player | player that send message |
| T | message |  |
| System.Threading.CancellationToken | cancellationToken |  |

##### Returns
| Type | Description |
| ---- | ---- |
| UniTask&lt;Mirage.Authentication.AuthenticationResult&gt; |  |

#### Authenticate(INetworkPlayer, T)


Called on server to Authenticate a message from client

Use  OR . 
By default the async version just call the normal version.




##### Declaration

```cs
protected virtual AuthenticationResult Authenticate(INetworkPlayer player, T message)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.INetworkPlayer | player |  |
| T | message |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.Authentication.AuthenticationResult |  |

#### SendAuthentication(NetworkClient, T)


Sends Authentication from client



##### Declaration

```cs
public void SendAuthentication(NetworkClient client, T message)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.NetworkClient | client |  |
| T | message |  |


