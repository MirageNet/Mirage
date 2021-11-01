# Remote Actions

The network system has ways to perform actions across the network. These type of actions are sometimes called Remote Procedure Calls. There are two types of RPCs in the network system, ServerRpc - which are called from the client and run on the server; and ClientRpc calls - which are called on the server and run on clients.

The diagram below shows the directions that remote actions take:

![Data Flow Graph](UNetDirections.jpg)



## Arguments to Remote Actions

The arguments passed to ServerRpc and ClientRpc calls are serialized and sent over the network. You can use any [supported Mirage type](../DataTypes.md).

Arguments to remote actions cannot be sub-components of game objects, such as script instances or Transforms.
