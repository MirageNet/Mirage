---
sidebar_position: 3
---
# Custom Authenticator

To create a custom Authenticator, follow these steps:

1. Inherit from the `NetworkAuthenticatorBase<T>` class.
2. Create a network message that your authenticator will receive from the client.
    - Clients should use the `SendAuthentication(NetworkClient client, T msg)` method provided by the authenticator to correctly send the authentication message.
4. Implement your authenticator to process this message and return a success or failure result.
5. Optionally, your authenticator can return additional data that you want to set on `INetworkPlayer.Authentication`.
6. Use the `GetData<T>()` method to retrieve the custom data on the server-side.


**Step 1: Inherit from `NetworkAuthenticatorBase<T>`**
{{{ Path:'Snippets/CustomAuthenticator.cs' Name:'authenticator-def' }}}

**Step 2: Create a Network Message**
{{{ Path:'Snippets/CustomAuthenticator.cs' Name:'auth-message' }}}

Clients should use the `SendAuthentication(NetworkClient client, T msg)` method to correctly send the authentication message.

:::note
Using `player.Send` directly will not work because the authenticator message is wrapped in an internal `AuthMessage` message.
:::

**Step 3: Implement the Authenticator**
{{{ Path:'Snippets/CustomAuthenticator.cs' Name:'authenticator' }}}

**Step 4: Return Additional Data (Optional)**
{{{ Path:'Snippets/CustomAuthenticator.cs' Name:'auth-data' }}}

**Step 5: Retrieve Custom Data**
{{{ Path:'Snippets/CustomAuthenticator.cs' Name:'use-data' }}}
