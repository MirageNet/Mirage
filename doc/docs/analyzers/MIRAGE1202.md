# MIRAGE1202: RPC Signature Error

## When this appears

A `[ServerRpc]` or `[ClientRpc]` has an unsupported method signature.

An unsupported signature stops the build because Weaver cannot generate the sending and receiving code for it. RPC methods must meet these requirements:

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

Choose one RPC attribute and a supported return type. Give the method a body and remove method-level generic parameters. Put optional payload defaults on a separate non-RPC wrapper and pass those arguments explicitly.

A generic `NetworkBehaviour` class and its concrete subclasses can still use RPCs. Parameters and results may use the class's type parameters or closed generic types. Each concrete payload type still needs registered readers and writers.

Missing serializers for a class-level `T` can make the RPC fail at runtime. For custom types, generate serializers or provide [custom serializers](./MIRAGE1301.md). For example, `[NetworkMessage]` can generate them for an eligible concrete type. Built-in `int` already has serializers.

{{{ Path:'Snippets/Analyzers/Mirage1202.cs' Name:'mirage1202-resolved' }}}
