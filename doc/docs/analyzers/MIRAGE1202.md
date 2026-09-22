# MIRAGE1202: RPC Signature Error

## The Problem
Methods decorated with `[ServerRpc]` or `[ClientRpc]` must follow these rules:
* **No Generic Methods:** The RPC method itself cannot declare generic parameters (e.g., `void MyRpc<T>()`).
* **No Abstract Methods:** An RPC must have a body. Use a concrete or virtual implementation instead.
* **One Direction:** A method cannot have both `[ServerRpc]` and `[ClientRpc]`.
* **No Optional Payload Parameters:** Serialized parameters cannot have default values. The exact `INetworkPlayer` context parameter is an exception: it may be optional in a ServerRpc, or in the first position of a ClientRpc. It supplies connection context rather than serialized data.
* **Valid Return Type:** Return `void`, or `Cysharp.Threading.Tasks.UniTask<T>` with a serializable result `T`. Non-generic `UniTask`, `UniTaskVoid`, `Task`, `Task<T>`, coroutine returns, and raw result values are not supported.

An Observers ClientRpc must return `void`, including when `[ClientRpc]` omits its target. Only ServerRpcs and Owner/Player ClientRpcs can return `UniTask<T>`; see [MIRAGE1205](./MIRAGE1205.md). These signature checks do not replace the separate rules for [static methods](./MIRAGE1204.md), [by-reference parameters](./MIRAGE1203.md), or [serializable payloads](./MIRAGE1301.md).

### Generics Rules
* **Allowed shape:** Generic NetworkBehaviour classes (e.g., `class MyBehaviour<T> : NetworkBehaviour`) and their concrete subclasses.
* **Allowed shape:** RPCs using type parameters from their enclosing class (e.g., `void MyRpc(T arg)`), including `UniTask<T>` results.
* **Allowed shape:** Closed generic payloads (e.g., `MyStruct<int>`) whose contents are serializable.
* **Disallowed:** RPC methods declaring their own generic parameters (e.g., `void MyRpc<T>(T arg)`). Mirage does not generate RPC dispatch for method-level generic instantiations.

“Allowed shape” is not a guarantee that every `T` works. The concrete payload type still needs registered readers and writers. Class-level type parameters use generic serialization at runtime; a missing registration can therefore fail when the RPC is used. For a concrete custom payload, arrange serializer generation (for example, with `[NetworkMessage]` on an eligible concrete type) or provide a supported custom pair. Built-in `int` already has serializers.

---

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1202.cs' Name:'mirage1202-triggering' }}}

---

## How to Resolve
* Remove generic parameters from the RPC method signature. Use class-level generic parameters or closed generic types instead.
* Give an abstract RPC a body; use `virtual` if it needs to be overridden.
* Choose one routing attribute and remove defaults from serialized parameters. Pass those arguments explicitly, or put convenience defaults on a separate non-RPC wrapper.
* Change the return type to `void` or a permitted `UniTask<T>` result, with readers and writers for `T`.

{{{ Path:'Snippets/Analyzers/Mirage1202.cs' Name:'mirage1202-resolved' }}}
