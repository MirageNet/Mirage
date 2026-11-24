---
id: PlayerAuthentication
title: PlayerAuthentication
---

# Class PlayerAuthentication



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
public class PlayerAuthentication
```

### Constructors

#### PlayerAuthentication(INetworkAuthenticator, Object)



##### Declaration

```cs
public PlayerAuthentication(INetworkAuthenticator authenticator, object data)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Authentication.INetworkAuthenticator | authenticator |  |
| System.Object | data |  |

### Fields

#### Authenticator

What Authenticator was used to accept this player
Null if no Authenticator existed on Server


##### Declaration

```cs
public readonly INetworkAuthenticator Authenticator
```
#### Data

Authentication data set by Authenticator when player is accepted


##### Declaration

```cs
public readonly object Data
```
### Methods
#### GetData&lt;T&gt;()


Helper method to cast  to type set by NetworkAuthenticatorBase
WARNING: this function is NOT thread safe when data is  rather than T directly



##### Declaration

```cs
public T GetData<T>()
```

##### Returns
| Type | Description |
| ---- | ---- |
| T |  |

