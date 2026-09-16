---
id: BasicAuthenticator
title: BasicAuthenticator
---

# Class BasicAuthenticator



<div class="inheritance">

##### Inheritance

<div class="level" style={{"--data-index": 0}}>
System.Object
</div>
<div class="level" style={{"--data-index": 1}}>
Mirage.Authentication.NetworkAuthenticator
</div>
<div class="level" style={{"--data-index": 2}}>
Mirage.Authentication.NetworkAuthenticator&lt;Mirage.Authenticators.BasicAuthenticator.JoinMessage&gt;
</div>
</div>

##### Inherited Members

<details>
<summary>Show</summary>

Mirage.Authentication.NetworkAuthenticator&lt;Mirage.Authenticators.BasicAuthenticator.JoinMessage&gt;.AuthenticateAsync(Mirage.INetworkPlayer, Mirage.Authenticators.BasicAuthenticator.JoinMessage, System.Threading.CancellationToken)


Mirage.Authentication.NetworkAuthenticator&lt;Mirage.Authenticators.BasicAuthenticator.JoinMessage&gt;.SendAuthentication(Mirage.NetworkClient, Mirage.Authenticators.BasicAuthenticator.JoinMessage)


Mirage.Authentication.NetworkAuthenticator.AuthenticatorName

</details>

##### Syntax

```cs
public class BasicAuthenticator : NetworkAuthenticator<BasicAuthenticator.JoinMessage>, INetworkAuthenticator
```


### Fields

#### ServerCode

##### Declaration

```cs
public string ServerCode
```
### Methods
#### Authenticate(INetworkPlayer, BasicAuthenticator.JoinMessage)



##### Declaration

```cs
protected override AuthenticationResult Authenticate(INetworkPlayer player, BasicAuthenticator.JoinMessage message)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.INetworkPlayer | player |  |
| Mirage.Authenticators.BasicAuthenticator.JoinMessage | message |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.Authentication.AuthenticationResult |  |

#### SendCode(NetworkClient, String)



##### Declaration

```cs
public void SendCode(NetworkClient client, string serverCode = null)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.NetworkClient | client |  |
| System.String | serverCode |  |


