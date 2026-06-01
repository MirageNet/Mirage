---
sidebar_position: 5
---
# RPC Examples

Examples of RPC and generated code.

## Example 1

Set a player's name from a client and have it synced to other players.

{{{ Path:'Snippets/RemoteActions/RpcExampleChangeName.cs' Name:'rpc-example-change-name' }}}

### Generated code

Weaver moves the user code into a new function and then replaces the body of the RPC with an internal send call.

RPCs are registered using the classes static constructor with methods that will read all the parameters and then invoke the user code method.

{{{ Path:'Snippets/RemoteActions/RpcExampleChangeNameGenerated.cs' Name:'rpc-example-change-name-generated' }}}