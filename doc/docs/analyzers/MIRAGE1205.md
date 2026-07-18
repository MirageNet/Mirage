# MIRAGE1205: Invalid ClientRpc Target Configurations

## The Problem
A `[ClientRpc]` target configuration is invalid if:
- The target is `RpcTarget.Observers` but the method return type is not `void`.
- The target is `RpcTarget.Player` but the first parameter is not an `INetworkPlayer`.

Broadcast RPCs (`RpcTarget.Observers`) cannot return values because multiple clients cannot return a single response. Returning values is only supported for specific targets like `RpcTarget.Owner` or `RpcTarget.Player`. Additionally, `RpcTarget.Player` requires an `INetworkPlayer` parameter so Mirage knows which client to target.

---

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1205.cs' Name:'mirage1205-triggering' }}}

---

## How to Resolve
- If the target is `RpcTarget.Observers`, change the return type to `void`, or use `RpcTarget.Owner` / `RpcTarget.Player` if returning values.
- If the target is `RpcTarget.Player`, add `INetworkPlayer` as the first parameter.

{{{ Path:'Snippets/Analyzers/Mirage1205.cs' Name:'mirage1205-resolved' }}}
