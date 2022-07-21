---
sidebar_position: 1
title: Overview
---
# Remote Actions Overview

To invoke code across the network you can use RPC (remote procedure call) and messages. 

RPC stands for  Remote Procedure Calls. They can be used inside [NetworkBehaviours](/docs/reference/Mirage/NetworkBehaviour) to tell either the client or server to do an action. For example, the client sends an RPC to the server to update the player's name.

There are 3 types of RPC:
- [Client Rpc](/docs/guides/remote-actions/client-rpc) | Called on the server, invoked on the client
- [Server Rpc](/docs/guides/remote-actions/server-rpc) | Called on the client, invoked on server, can have return values
- [Network Messages](/docs/guides/remote-actions/network-messages) | Calls on either the server/client and require a handler to be registered

Mirage uses [Network messages](/docs/guides/remote-actions/network-messages) for sending everything, this includes Spawning, RPC, and SyncVars. Network message serialized into bytes then sent over the network. 

Network Message can be used to send data or invoke actions without a NetworkBehaviour. For example, sending character select information before the player's character is spawned. 

The diagram below shows the directions that remote actions take:

:::note
"Commands" is the previous name for "ServerRpc"
:::

![Data Flow Graph](/img/guides/remote-actions/unet-directions.jpg)

## Arguments to Remote Actions

Mirage serializes RPC arguments to send them over the network. You can use any [supported Mirage type](/docs/guides/data-types).

There are limits to what can be arguments. GameObject, NetworkIdentity, and NetworkBehaviour can be sent because they have a Network ID. But, Mirage can't send other Unity Objects by itself because it will have no way to find them on the other side.

It is also possible to create serializer functions for unsupported types. You can find out more information [here](/docs/guides/data-types#custom-data-types).
