# MIRAGE1205: Invalid ClientRpc Target Configurations

## The Problem
A `[ClientRpc]` target configuration is invalid for one of the following reasons:
1. The target is set to `RpcTarget.Observers` but the method return type is not `void`.
2. The target is set to `RpcTarget.Player` but the first parameter of the method is not an `INetworkPlayer` to specify the recipient.

Broadcast RPCs (where the target is `Observers`) must return `void` because multiple clients would respond, making return values or tasks invalid. Returning values requires a single, specific destination (e.g. `RpcTarget.Owner` or `RpcTarget.Player`). Furthermore, when targeting a specific `Player`, Mirage needs to know which connection to send the RPC to, so the method's first parameter must be the player connection (`INetworkPlayer`).

---

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1205.cs' Name:'mirage1205-triggering' }}}

---

## How to Resolve

1. If the RPC target is `RpcTarget.Observers`, change the return type to `void`. Alternatively, if the RPC returns values, change the target to `RpcTarget.Owner` or `RpcTarget.Player`.
2. If the RPC targets `RpcTarget.Player`, ensure the first parameter is of type `INetworkPlayer`.

{{{ Path:'Snippets/Analyzers/Mirage1205.cs' Name:'mirage1205-resolved' }}}
