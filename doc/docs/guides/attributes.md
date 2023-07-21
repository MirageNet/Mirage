---
sidebar_position: 4
---
# Attributes

Networking attributes are added to member of [NetworkBehaviour](/docs/reference/Mirage/
NetworkBehaviour) scripts to tell Mirage to do different things.

There are 4 types of attributes that Mirage has:
- [Rpc Attributes](#rpc-attributes): Cause a method to send a network message so that the body of the method is invoked on a either server or client
- [Block methods invokes](#block-methods-invokes): Blocks invokes of a method to stop them from being invoked in wrong place
- [SyncVar](/docs/guides/sync/sync-var): Add to Fields to cause their value to be automatically synced to clients.
- [Bit Packing](/docs/guides/bit-packing): These attributes modify how values are written, they are an easy way to tell mirage how to compress values before they are sent over network. They can be applied to Fields and method Parameters.

## RPC Attributes 

Full details on RPC can be found on the [Remote Actions](/docs/guides/remote-actions) page.

:::note
When using abstract or virtual methods the attributes need to be applied to the override methods too.
:::

-   **[ClientRpcAttribute](/docs/reference/Mirage/ClientAttribute)**  
    The server uses a Remote Procedure Call (RPC) to run that function on clients. It has a `target` option allowing 
    you to specify in which clients it should be executed, along with a `channel` option. 
    See also: [ClientRpc](/docs/guides/remote-actions/client-rpc)

-   **[ServerRpcAttribute](/docs/reference/Mirage/ServerRpcAttribute)**  
    Call this from a client to run this function on the server. Make sure to validate the input on the server. 
    It's not possible to call this from a server. Use this as a wrapper around another function, if you want to call it 
    from the server too. Note that you can also return value from it. See also: [ServerRpc](/docs/guides/remote-actions/server-rpc)


## Block methods invokes

These attributes can be added to methods to block them from being invoked in the wrong place. These attributes can only be used on `NetworkBehaviour` classes, and only work correctly if the object is spawned. otherwise flags like `IsServer` will be false.

By default these methods will throw [MethodInvocationException](/docs/reference/Mirage/MethodInvocationException). You can add `error = false` to return instead of throw. 

:::note
When returning early the method will return default values for return value or for out param
:::

These attributes can be used for Unity game loop methods like `Start` or `Update`, as well as other implemented methods.


-   **[ServerAttribute](/docs/reference/Mirage/ServerAttribute)**  
    Methods can only be invoked on Server

-   **[ClientAttribute](/docs/reference/Mirage/ClientAttribute)**  
    Methods can only be invoked on Client

-   **[HasAuthorityAttribute](/docs/reference/Mirage/HasAuthorityAttribute)**  
    Methods can only be invoked on Client, when HasAuthority is true. See: [Authority](/docs/guides/authority)

-   **[LocalPlayerAttribute](/docs/reference/Mirage/LocalPlayerAttribute)**  
    Methods can only be invoked on Client, when IsLocalPlayer is true. See: [Authority](/docs/guides/game-objects/spawn-player)

-   **[NetworkMethodAttribute](/docs/reference/Mirage/NetworkMethodAttribute)**  
    Method can only be invoked based on the flags set in the attribute. For example `NetworkFlags.Server | NetworkFlags.HasAuthority` can only be called on Server **OR** on client with Authority.

#### Examples:

```cs
[Server]
void SpawnCoin() 
{
    // only allowed to be invoked on server
}
```

```cs
[NetworkMethod(NetworkFlags.Server | NetworkFlags.NotActive)]
public void StartGame()
{
    // this methods will run in server or single player mode
    // it will only be blocks if only client is active
}
```
