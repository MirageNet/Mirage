# MIRAGE1205: Invalid ClientRpc Target Configurations

## The Problem
A `[ClientRpc]` target configuration is invalid if:
- The target is `RpcTarget.Observers` but the method return type is not `void`. Observers is also the default when `target` is omitted.
- The target is `RpcTarget.Player` but the first parameter is not the target connection. Use `Mirage.INetworkPlayer` as its declared type.
- The target is `RpcTarget.Owner` and `excludeOwner = true`.

Observers RPCs may have multiple recipients and cannot return a single response. Owner and Player RPCs may return `void` or `Cysharp.Threading.Tasks.UniTask<T>` with a serializable result; they do not allow arbitrary return types. See [MIRAGE1202](./MIRAGE1202.md).

Owner selects the object's owner; Player selects the connection passed to the first parameter. The target client must have the corresponding spawned identity to execute the RPC. A missing owner causes a targeted send to fail. Pass an explicit non-null player for Player routing; the remote send path has an owner fallback for null, but the host local-call check uses the supplied value directly.

The first `INetworkPlayer` is not serialized. On the receiving client it is connection context for the server, not a copy of the caller's player object. Owner/Observers RPCs may also have this first connection-context parameter without changing their routing. Additional or later `INetworkPlayer` parameters are not supported on ClientRpcs. A ServerRpc's injected sender parameter has different rules and may appear in any position.

Use the exact `INetworkPlayer` interface in these signatures. Although the Weaver's Player-target precheck accepts implementing types, other serialization and injection steps use an exact-type check; that precheck alone does not establish support for concrete implementations or derived interfaces.

---

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1205.cs' Name:'mirage1205-triggering' }}}

---

## How to Resolve
- If the target is `RpcTarget.Observers`, change the return type to `void`, or use `RpcTarget.Owner` / `RpcTarget.Player` with a valid `UniTask<T>` result.
- If the target is `RpcTarget.Player`, add `INetworkPlayer` as the first parameter.
- If the target is `RpcTarget.Owner`, remove `excludeOwner = true`. Use Observers with `excludeOwner = true` only when the intended recipients are the other observers.

{{{ Path:'Snippets/Analyzers/Mirage1205.cs' Name:'mirage1205-resolved' }}}
