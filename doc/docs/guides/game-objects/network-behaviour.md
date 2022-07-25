---
sidebar_position: 2
---
# Network Behaviour

**See also [NetworkBehaviour](/docs/reference/Mirage/NetworkBehaviour) in the API Reference.**

Network Behaviour scripts work with game objects that have a NetworkIdentity component. These scripts can perform high-level API functions such as ServerRpcs, ClientRpcs, and SyncVars.

With the server-authoritative system of Mirage, the server must use the `NetworkServer.Spawn` function to spawn game objects with Network Identity components. Spawning them this way assigns them a `netId` and creates them on clients connected to the server.

**Note:** This is not a component that you can add to a game object directly. Instead, you must create a script that inherits from `NetworkBehaviour` (instead of the default `MonoBehaviour`), then you can add your script as a component to a game object.

NetworkBehaviour scripts have the following features:
- [Synchronized variables](#synchronized-variables)
- [Server and Client functions](#server-and-client-functions)
- [Server RPC Calls](#server-rpc-calls)
- [Client RPC Calls](#client-rpc-calls)
- [Network Callbacks](#network-callbacks)

![Data Flow Graph](/img/guides/remote-actions/unet-directions.jpg)

**Note:** NetworkBehaviors in Mirror and in UNet provide virtual functions as a way for you to add logic in response to lifecycle events.  Mirage does not,  instead add listeners to the events in [NetworkIdentity](/docs/components/network-identity).

## Synchronized variables

Your component can have data that is automatically synchronized from the server to the client. You can use [SyncVars](/docs/guides/sync/sync-var) as well as [SyncLists](/docs/guides/sync/sync-list), [SyncHashSet](/docs/guides/sync/sync-hash-set), and [SyncDictionary](/docs/guides/sync/sync-dictionary) inside a NetworkBehaviour. They will be automatically propagated to the clients whenever their value changes in the server.
 
## Server and Client functions

You can tag member functions in NetworkBehaviour scripts with custom attributes to designate them as server-only or client-only functions. [ServerAttribute](/docs/reference/Mirage/ServerAttribute) 
will check that the function is called in the server. Likewise, [ClientAttribute](/docs/reference/Mirage/ClientAttribute) will check if the function is called in the client.

For more information, see [Attributes](/docs/guides/attributes).

## Server RPC Calls

To execute code on the server, you must use Server RPC calls. The high-level API is a server-authoritative system, so ServerRpc is the only way for a client to trigger some code on the server.

Only player game objects can send ServerRpcs.

When a client player game object sends a ServerRpc, that ServerRpc runs on the corresponding player game object on the server. This routing happens automatically, so it is impossible for a client to send a ServerRpc for a different player.

To define a Server RPC Call in your code, your function must have a [`ServerRpc`](/docs/reference/Mirage/ServerRpcAttribute) attribute.

Server RPC Calls are called just by invoking the function normally on the client. Instead of the ServerRpc function running on the client, it is automatically invoked on the corresponding player game object on the server.

Server RPC Calls are type-safe, have built-in security and routing to the player, and use an efficient serialization mechanism for the arguments to make calling them fast.

See [Server RPC](/docs/guides/remote-actions/server-rpc) and related sections for more information.

## Client RPC Calls

Client RPC calls are a way for server game objects to make things happen on client game objects.

Client RPC calls are not restricted to player game objects and may be called on any game object with a Network Identity component.

To define a Client RPC call in your code, your function must have a [`ClientRpc`](/docs/reference/Mirage/ClientRpcAttribute) attribute.

See [Client RPC](/docs/guides/remote-actions/client-rpc) and related sections for more information.

## Network Callbacks

Callbacks can be used to make sure code is executed at the right time.

The network callbacks are found inside [NetworkIdentity](/docs/reference/Mirage/NetworkIdentity) so they can also be used outside of a NetworkBehaviour.

See [NetworkBehaviour Callbacks](/docs/guides/callbacks/network-behaviour) and related sections for more information.

