---
id: Mirage.RemoteCalls
title: Mirage.RemoteCalls
---

# Mirage.RemoteCalls

## Classes

#### [ClientRpcSender](./ClientRpcSender)
#### [RemoteCall](./RemoteCall)
> 
Used for invoking a RPC methods

#### [RemoteCallCollection](./RemoteCallCollection)
#### [RemoteCallCollectionCache](./RemoteCallCollectionCache)
> 
Caches immutable RPC collection layouts to avoid re-allocating delegate wrappers and array buffers per spawned identity

#### [ReturnRpcException](./ReturnRpcException)
#### [ServerRpcSender](./ServerRpcSender)
> 
Methods used by weaver to send RPCs

## Structs

#### [RpcId](./RpcId)
#### [RpcMessage](./RpcMessage)
#### [RpcRateLimitConfig](./RpcRateLimitConfig)
> 
Rate limit configuration for an RPC method.
Use  for no rate limiting, or  to configure a token bucket.

#### [RpcReply](./RpcReply)
#### [RpcWithReplyMessage](./RpcWithReplyMessage)
## Enums

#### [RpcInvokeType](./RpcInvokeType)
## Delegates

#### [RequestDelegate&lt;T&gt;](./RequestDelegate-1)
#### [RpcDelegate](./RpcDelegate)
> 
Delegate for ServerRpc functions.

