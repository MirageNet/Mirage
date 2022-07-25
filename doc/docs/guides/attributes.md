---
sidebar_position: 4
---
# Attributes

Networking attributes are added to member functions of [NetworkBehaviour](/docs/reference/Mirage/NetworkBehaviour) 
scripts, to make them run on either the client or server.

These attributes can be used for Unity game loop methods like `Start` or `Update`, as well as other implemented methods.

:::note
When using abstract or virtual methods the attributes need to be applied to the override methods too.
:::

-   **[ServerAttribute](/docs/reference/Mirage/ServerAttribute)**  
    Only a server can call the method (throws an error when called on a client unless you specify `error = false`).

-   **[ClientAttribute](/docs/reference/Mirage/ClientAttribute)**  
    Only a client can call the method (throws an error when called on the server unless you specify `error = false`).

-   **[ClientRpcAttribute](/docs/reference/Mirage/ClientAttribute)**  
    The server uses a Remote Procedure Call (RPC) to run that function on clients. It has a `target` option allowing 
    you to specify in which clients it should be executed, along with a `channel` option. 
    See also: [ClientRpc](/docs/guides/remote-actions/client-rpc)

-   **[ServerRpcAttribute](/docs/reference/Mirage/ServerRpcAttribute)**  
    Call this from a client to run this function on the server. Make sure to validate the input on the server. 
    It's not possible to call this from a server. Use this as a wrapper around another function, if you want to call it 
    from the server too. Note that you can also return value from it. See also: [ServerRpc](/docs/guides/remote-actions/server-rpc)

-   **[SyncVar](/docs/guides/sync/sync-var)**  
    SyncVars are used to synchronize a variable from the server to all clients automatically.