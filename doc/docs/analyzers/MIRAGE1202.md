# MIRAGE1202: RPC Signature Error

## When this appears

A `[ServerRpc]` or `[ClientRpc]` violates these signature requirements:

| Requirement | Valid signature |
| --- | --- |
| Direction | Exactly one of `[ServerRpc]` or `[ClientRpc]`. |
| Body | A concrete implementation; `virtual` is allowed, `abstract` is not. |
| Generics | No generic parameters declared by the RPC method itself. |
| Defaults | No optional serialized parameters. Exact `INetworkPlayer` context parameters may be optional: anywhere in a ServerRpc, or first in a ClientRpc. |
| Return | `void` or `Cysharp.Threading.Tasks.UniTask<T>` with a serializable result `T`. Observers ClientRpcs, including the default target, must return `void`. |

Non-generic `UniTask`, `UniTaskVoid`, `Task`, `Task<T>`, coroutine returns, and raw results are unsupported. Only ServerRpcs and Owner/Player ClientRpcs can return `UniTask<T>`; see [target requirements](./MIRAGE1205.md).

[Static methods](./MIRAGE1204.md), [by-reference parameters](./MIRAGE1203.md), and [unsupported payloads](./MIRAGE1301.md) have separate rules.

{{{ Path:'Snippets/Analyzers/Mirage1202.cs' Name:'mirage1202-triggering' }}}

## How to fix

Apply the requirements above. Put convenience defaults on a separate non-RPC wrapper and pass payload arguments explicitly.

Generic `NetworkBehaviour` classes, concrete subclasses, enclosing type parameters in parameters or results, and closed generic payloads are supported shapes. Every concrete payload still needs registered readers and writers.

Missing registration for a class-level `T` can fail when the RPC runs. For custom concrete types, arrange generation, such as `[NetworkMessage]` on an eligible concrete type, or supply supported custom serializers. Built-in `int` already has serializers.

{{{ Path:'Snippets/Analyzers/Mirage1202.cs' Name:'mirage1202-resolved' }}}
