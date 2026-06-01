---
sidebar_position: 3
---
# Server RPC

Server RPC Calls are sent from character objects on the client to character objects on the server. For security, Server RPC Calls can only be sent from YOUR character object by default, so you cannot control the objects of other players. You can bypass the authority check using `[ServerRpc(requireAuthority = false)]`.

To make a function into a Server RPC call, add the [ServerRpc] custom attribute to it. This function will now be run on the server when it is called on the client. Any parameters of the [allowed data types](/docs/guides/serialization/data-types) will be automatically passed to the server with the Server RPC Call.

Server RPC Calls functions cannot be static. 

{{{ Path:'Snippets/RemoteActions/ServerRpcDropCube.cs' Name:'server-rpc-drop-cube' }}}

:::caution
Be careful of sending ServerRpcs from the client every frame! This can cause a lot of network traffic.
:::

### Returning values

ServerRpcs can return values. It can take a long time for the server to reply, so they must return a UniTask which the client can await.
To return a value, add a return value using `UniTask<MyReturnType>` where `MyReturnType` is any [supported Mirage type](/docs/guides/serialization/data-types). In the server, you can make your method async,  or you can use `UniTask.FromResult(myResult);`. For example:

{{{ Path:'Snippets/Rpc/RpcReply.cs' Name:'server-rpc-reply' }}}

### ServerRpc and Authority

It is possible to invoke ServerRpcs on non-character objects if any of the following are true:

- The object was spawned with client authority
- The object has client authority set with `NetworkIdentity.AssignClientAuthority`
- the Server RPC Call has the `requireAuthority` option set false.  
    - You can include an optional `INetworkPlayer sender = null` parameter in the Server RPC Call method signature and Mirage will fill in the sending client for you.
    - Do not try to set a value for this optional parameter...it will be ignored.

Server RPC Calls sent from these objects are run on the server instance of the object, not on the associated character object for the client.

{{{ Path:'Snippets/RemoteActions/ServerRpcDoor.cs' Name:'server-rpc-door' }}}

## Protecting Against Memory Allocation Attacks (MaxLength Attribute)

When receiving data from untrusted clients, malicious users could send a serialized payload that specifies an extremely large size for strings or collections (such as arrays or lists). By default, network libraries read this length header first and immediately allocate an array or string of that size in memory (e.g., `new T[size]`). This can result in Out of Memory (OOM) crashes or high CPU overhead on the server, acting as a Denial of Service (DoS) attack vector.

To protect against this, Mirage provides the `[MaxLength(int)]` attribute which can be applied directly to `ServerRpc` parameters:

```cs
[ServerRpc]
public void CmdSendPlayerName([MaxLength(32)] string newName)
{
    // The name is guaranteed to be 32 characters or fewer
    playerName = newName;
}

[ServerRpc]
public void CmdSendInventory([MaxLength(100)] int[] itemIds)
{
    // The array is guaranteed to contain 100 elements or fewer
    ProcessInventory(itemIds);
}
```

### How It Works

1. **Early Size Verification:**
   - For collections (arrays/lists), Mirage reads the count from the network stream. If the count exceeds the specified `maxLength`, it throws a `SerializationLimitException` **before** allocating memory for the elements.
   - For strings, Mirage checks if the incoming byte size (`realSize`) is greater than `maxLength * 4` (since a UTF-8 character is at most 4 bytes). If this check fails, or if the decoded character length exceeds `maxLength`, a `SerializationLimitException` is thrown.
2. **Flagging and Penalty:**
   - When a `SerializationLimitException` is thrown, the server catches it in the RPC/message handler.
   - The connection is flagged with `PlayerErrorFlags.SerializationLimit`.
   - A penalty cost of `100` is applied to the player's error rate limit budget. By default, exceeding the budget triggers an automatic disconnection, disconnecting the malicious client.
