# MIRAGE1205: Invalid ClientRpc Target Configurations

## When this appears

A `[ClientRpc]` does not meet its target's requirements:

| Target | Requirement |
| --- | --- |
| `RpcTarget.Observers` (default) | Return `void`; multiple recipients cannot provide one result. |
| `RpcTarget.Player` | Declare `Mirage.INetworkPlayer` as the first parameter. |
| `RpcTarget.Owner` | Do not set `excludeOwner = true`. |

Owner/Player RPCs may return `void` or `Cysharp.Threading.Tasks.UniTask<T>` with serializable `T`. Other return types remain [unsupported](./MIRAGE1202.md).

{{{ Path:'Snippets/Analyzers/Mirage1205.cs' Name:'mirage1205-triggering' }}}

## How to fix

Use `void` for Observers, or choose Owner/Player for a result. Add the first `INetworkPlayer` parameter for Player routing. Remove `excludeOwner = true` for Owner; use Observers with that option when the intended recipients are the other observers.

{{{ Path:'Snippets/Analyzers/Mirage1205.cs' Name:'mirage1205-resolved' }}}

### Target and connection context

- Owner selects the object's owner; Player selects the supplied connection. The client needs the corresponding spawned identity. Sending to a missing owner fails.
- Pass a non-null Player target: remote sending can fall back to the owner for null; host local-call selection uses the supplied value.
- Only the first ClientRpc parameter may be `INetworkPlayer` context, including Owner/Observers without changing routing. It is not serialized; the receiving body gets its connection to the server.
- Use the exact interface, excluding concrete implementations and derived interfaces. ServerRpc sender context can appear anywhere; see [optional context rules](./MIRAGE1202.md).
