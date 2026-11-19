---
id: RpcWithReplyMessage
title: RpcWithReplyMessage
---

# Struct RpcWithReplyMessage




##### Syntax

```cs
public struct RpcWithReplyMessage
```


### Fields

#### NetId

##### Declaration

```cs
public uint NetId
```
#### FunctionIndex

##### Declaration

```cs
public int FunctionIndex
```
#### ReplyId

Id sent with rpc so that server can reply with  and send the same Id


##### Declaration

```cs
public int ReplyId
```
#### Payload

##### Declaration

```cs
public ArraySegment<byte> Payload
```
