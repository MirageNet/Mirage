# MIRAGE1205: Invalid ClientRpc Target Configurations

## When this appears

A `[ClientRpc]` target conflicts with its return type, target parameter, or `excludeOwner` setting.

| Target | Invalid setting | Why it fails |
| --- | --- | --- |
| `RpcTarget.Observers` (default) | Return type is not `void`. | Multiple recipients cannot provide one result. |
| `RpcTarget.Player` | First parameter is not `Mirage.INetworkPlayer`. | Player routing needs a connection to send to. |
| `RpcTarget.Owner` | `excludeOwner = true`. | This excludes the only selected recipient. |

Owner/Player RPCs may return `void` or `Cysharp.Threading.Tasks.UniTask<T>` with serializable `T`. Other return types remain [unsupported](./MIRAGE1202.md).

{{{ Path:'Snippets/Analyzers/Mirage1205.cs' Name:'mirage1205-triggering' }}}

## How to fix

Use `void` for Observers, or choose Owner/Player for a result. Add the first `INetworkPlayer` parameter for Player routing.

Remove `excludeOwner = true` for Owner. Use Observers with that option when the intended recipients are the other observers.

{{{ Path:'Snippets/Analyzers/Mirage1205.cs' Name:'mirage1205-resolved' }}}

### Target and connection context

- Owner selects the object's owner; Player selects the supplied connection. The client needs the corresponding spawned identity. Sending to a missing owner fails.
- Pass a non-null Player target. Remote sending can fall back to the owner for null; deciding whether to run on the host uses the supplied value instead.
- Any ClientRpc target may use `INetworkPlayer` as its first parameter. It is not serialized; the receiving method gets its connection to the server. Adding this parameter does not change Owner/Observers routing.
- Use the exact `INetworkPlayer` interface, not a concrete class or derived interface. Later ClientRpc parameters cannot use it. A ServerRpc sender parameter can appear anywhere; see [optional parameter rules](./MIRAGE1202.md).
