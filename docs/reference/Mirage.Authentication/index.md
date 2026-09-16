---
id: Mirage.Authentication
title: Mirage.Authentication
---

# Mirage.Authentication

## Classes

#### [AuthenticatorSettings](./AuthenticatorSettings)
#### [AuthenticatorSettings.PendingAuth](./AuthenticatorSettings.PendingAuth)
#### [NetworkAuthenticator](./NetworkAuthenticator)
#### [NetworkAuthenticator&lt;T&gt;](./NetworkAuthenticator-1)
#### [PlayerAuthentication](./PlayerAuthentication)
## Structs

#### [AuthMessage](./AuthMessage)
> 
Wrapper message around auth message sent by a 

This type is used to that it can be receive before player is authenticated.
ALl AuthMessage will be handled by an Authenticator instead of the normal message handler


#### [AuthSuccessMessage](./AuthSuccessMessage)
#### [AuthenticationResult](./AuthenticationResult)
> 
Result from Authentication, Use static methods to create new instance

## Interfaces

#### [IAuthenticationDataWrapper](./IAuthenticationDataWrapper)
> 
Auth data might be a wrapper around another Authenticator&apos;s data.
In that case  should check if data is T or if it is IDataWrapper

#### [INetworkAuthenticator](./INetworkAuthenticator)
