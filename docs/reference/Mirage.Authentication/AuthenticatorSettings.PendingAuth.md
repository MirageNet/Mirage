---
id: AuthenticatorSettings.PendingAuth
title: AuthenticatorSettings.PendingAuth
---

# Class AuthenticatorSettings.PendingAuth



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
public class PendingAuth
```


### Fields

#### CancelSource

##### Declaration

```cs
public readonly CancellationTokenSource CancelSource
```
### Methods
#### SetResult(AuthenticationResult)



##### Declaration

```cs
public void SetResult(AuthenticationResult result)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Authentication.AuthenticationResult | result |  |


#### WaitWithTimeout(Single)



##### Declaration

```cs
public UniTask<AuthenticationResult> WaitWithTimeout(float timeoutSecond)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Single | timeoutSecond |  |

##### Returns
| Type | Description |
| ---- | ---- |
| UniTask&lt;Mirage.Authentication.AuthenticationResult&gt; |  |

