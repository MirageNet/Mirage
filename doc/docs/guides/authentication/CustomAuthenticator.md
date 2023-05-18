# Custom Authenticator

To create an Authenticator you need too inherit from `NetworkAuthenticatorBase<T>`

First you need to create a network message that your authenticator will receive from the client

{{{ Path:'Snippets/CustomAuthenticator.cs' Name:'auth-message' }}}

Then create your Authenticator to process this message and return success or fail result.

`NetworkAuthenticatorBase<T>` has both Synchronous vs Asynchronous of `Authenticate`. You must override one of them, but not both. By default the async version calls the Synchronous version and returns it's result instantly.

{{{ Path:'Snippets/CustomAuthenticator.cs' Name:'authenticator' }}}

Your Authenticator can also return data that you want to set on `INetworkPlayer.Authentication`

{{{ Path:'Snippets/CustomAuthenticator.cs' Name:'auth-data' }}}

You can then use that data using the `GetData<T>()` method

{{{ Path:'Snippets/CustomAuthenticator.cs' Name:'use-data' }}}


Clients should use the `SendAuthentication(NetworkClient client, T msg)` methods in order to correctly send the authentication message, 

:::note
Using `player.Send` will not work because authenticator message is wrapped in internal `AuthMessage` message.
:::