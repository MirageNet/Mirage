# Remote Actions

To invoke code across the network you can use RPC and messages. 

RPC stands for  Remote Procedure Calls. They can be used inside [NetworkBehaviours](xref:Mirage.NetworkBehaviour) to tell either the client or server to do an action. For example, the client sending an RPC to the server to update the player's name.

There are 2 types of RPC:
- [ClientRpc](./ClientRpc.md) | Called on server, Invoked on client
- [ServerRpc](./ServerRpc.md) | Called on client, Invoked on server


Mirage uses network Messages for sending everything, this includes Spawning, RPC, and SyncVars. Network message serialized into bytes then sent over the network. 

Network Message can be used to send data or invoke actions without a NetworkBehaviours. For example, sending character select information before the player's character is spawned. 

The diagram below shows the directions that remote actions take:

![Data Flow Graph](UNetDirections.jpg)

>[!NOTE] 
> "Commands" is the previous name for "ServerRpc"

## Arguments to Remote Actions

Mirage serializes RPC arguments to send them over the network. You can use any [supported Mirage type](../DataTypes.md).

There are limits to what can be arguments. GameObject, NetworkIdentity, and NetworkBehaviour can be sent because they have a Network ID. But, Mirage can't send other Unity Objects by itself because it will have no way to find them on the other side.

It is also possible to create serialize functions for unsupported types. You can find out more information [here](../DataTypes.md#custom-data-types).
