---
sidebar_position: 4
---
# Attributes

Networking attributes are added to members of [NetworkBehaviour](/docs/reference/Mirage/NetworkBehaviour) scripts to tell Mirage to do different things.

There are 4 types of attributes that Mirage has:
- **[RPC Attributes](#rpc-attributes)**: Cause a method to send a network message so that the body of the method is invoked on either the server or client.
- **[Block methods invokes](#block-methods-invokes)**: Attributes used to restrict method invocation to specific contexts.
- **[SyncVar](/docs/guides/sync/sync-var)**: Add to Fields to cause their value to be automatically synced to clients.
- **[Bit Packing](/docs/guides/bit-packing)**: These attributes modify how values are written, providing an easy way to compress values before they are sent over the network. They can be applied to Fields and method Parameters.


## RPC Attributes

Full details on RPC can be found on the [Remote Actions](/docs/guides/remote-actions) page.

Both Rpc attributes support setting the channel to Reliable or Unreliable.

:::note
When using abstract or virtual methods, the attributes need to be applied to the override methods too.
:::

-   **[ClientRpcAttribute](/docs/reference/Mirage/ClientAttribute)**  
    The `ClientRpcAttribute` allows the server to use a Remote Procedure Call (RPC) to run a function on specific clients, with options to target the owner, all observers, or a specified player.
    See also: [ClientRpc](/docs/guides/remote-actions/client-rpc)

-   **[ServerRpcAttribute](/docs/reference/Mirage/ServerRpcAttribute)**  
    The `ServerRpcAttribute` is used when you want to call a function on the server from a client. Make sure to validate the input on the server. Note that you cannot call this attribute from the server itself. You can use this attribute as a wrapper around another function, allowing you to call it from the server as well. Additionally, you can return values from functions marked with this attribute. 
    See also: [ServerRpc](/docs/guides/remote-actions/server-rpc)

## Block Methods Invokes

These attributes can be added to methods to block them from being invoked in the wrong place. These attributes can only be used in `NetworkBehaviour` classes and when the object is spawned. If the object is not spawned, all the flags like `IsServer` will be false so will block the methods even if the server is running.

By default, methods with these attributes will throw a `MethodInvocationException` if invoked improperly. However, you can add `error = false` to return instead of throwing an exception.

:::note
When a method returns early due to a blocked invocation, the method will return default values for the return value or out parameters.
:::

These attributes can be used for Unity game loop methods like `Start`, `Update` or `OnCollisionEnter`, as well as other implemented methods that need to be restricted to certain contexts.

#### Available Attributes:

- **[ServerAttribute](#server-attribute):** Methods can only be invoked on the server.
- **[ClientAttribute](#client-attribute):** Methods can only be invoked on the client.
- **[HasAuthorityAttribute](#has-authority-attribute):** Methods can only be invoked on the client when the player has authority of the object. See: [Authority](/docs/guides/authority)
- **[LocalPlayerAttribute](#local-player-attribute):** Methods can only be invoked on the client when the object is the local player. See: [Authority](/docs/guides/game-objects/spawn-player)
- **[NetworkMethodAttribute](#network-method-attribute):** Methods can only be invoked based on the flags set in the attribute. For example, `NetworkFlags.Server | NetworkFlags.HasAuthority` allows the method to be called on the server **OR** on the client with authority.

#### Examples:

```cs
[Server]
void SpawnCoin() 
{
    // This method is only allowed to be invoked on the server.
}
```

```cs
[NetworkMethod(NetworkFlags.Server | NetworkFlags.NotActive)]
public void StartGame()
{
    // This method will run on the server or in single-player mode.
    // It will only be blocked if the client is active.
}
```