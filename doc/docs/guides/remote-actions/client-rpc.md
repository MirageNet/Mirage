---
sidebar_position: 2
---
# Client RPC

ClientRpcs are sent from [NetworkBehaviours](/docs/reference/Mirage/NetworkBehaviour) on the server to Behaviours on the client. They can be sent from any [NetworkBehaviour](/docs/reference/Mirage/NetworkBehaviour) that has been spawned.

To make a function into a ClientRpc add [`[ClientRpc]`](/docs/reference/Mirage/ClientRpcAttribute) directly above the function.

{{{ Path:'Snippets/RemoteActions/ClientRpcExamples.cs' Name:'client-rpc-attribute' }}}

ClientRpc functions can't be static and must return `void`.

## RpcTarget

There are 3 target modes for ClientRpc:
- Observers (default)
- Owner
- Player

### RpcTarget.Observers

This is the default target.

This will send the RPC message to only the observers of an object according to its [Network Visibility](/docs/guides/network-visibility). If there is no Network Visibility on the object it will send to all players.

### RpcTarget.Owner

This will send the RPC message to only the owner of the object.

### RpcTarget.Player

This will send the RPC message to the [`NetworkPlayer`](/docs/reference/Mirage/NetworkPlayer) that is passed into the call.

{{{ Path:'Snippets/RemoteActions/ClientRpcExamples.cs' Name:'client-rpc-player' }}}

Mirage will use the `NetworkPlayer target` to know where to send it, but it will not send the `target` value. Because of this, its value will always be null for the client.

## Exclude owner

You may want to exclude the owner client when calling a ClientRpc. This is done with the `excludeOwner` option: `[ClientRpc(excludeOwner = true)]`.


## Channel

RPC can be sent using either the Reliable or Unreliable channels. `[ClientRpc(channel = Channel.Reliable)]`

### Returning values

ClientRpcs can return values only if RpcTarget is `Player` or `Owner`. It can take a long time for the client to reply, so they must return a UniTask which the server can await.

To return a value, add a return value using `UniTask<MyReturnType>` where `MyReturnType` is any [supported Mirage type](/docs/guides/serialization/data-types). In the client, you can make your method async,  or you can use `UniTask.FromResult(myResult);`. For example:

{{{ Path:'Snippets/Rpc/RpcReply.cs' Name:'client-rpc-reply' }}}

# Examples 

{{{ Path:'Snippets/RemoteActions/ClientRpcExamples.cs' Name:'client-rpc-example-health' }}}

When running a game as a host with a local client, ClientRpc calls will be invoked on the local client even though it is in the same process as the server. So the behaviors of local and remote clients are the same for ClientRpc calls.

You can also specify which client gets the call with the `target` parameter. 

If you only want the client that owns the object to be called,  use `[ClientRpc(target = RpcTarget.Owner)]` or you can specify which client gets the message by using `[ClientRpc(target = RpcTarget.Player)]` and passing the player as a parameter.  For example:

{{{ Path:'Snippets/RemoteActions/ClientRpcExamples.cs' Name:'client-rpc-example-magic' }}}

## Parameter Size Limits (MaxLength Attribute)

Just like ServerRpcs, you can use the `[MaxLength(int)]` attribute on `ClientRpc` string and collection parameters to restrict the maximum allowed size of incoming payloads during deserialization. See the [Server Rpc MaxLength documentation](/docs/guides/remote-actions/server-rpc#protecting-against-memory-allocation-attacks-maxlength-attribute) for details.
