# MIRAGE1202: RPC Signature Error

## The Problem
Methods decorated with `[ServerRpc]` or `[ClientRpc]` must follow these rules:
* **No Generic Methods:** The RPC method itself cannot declare generic parameters (e.g., `void MyRpc<T>()`).
* **Valid Return Type:** The method must return `void`, `UniTask`, or `UniTask<T>`.

### Generics Rules
* **Allowed:** Generic NetworkBehaviour classes (e.g., `class MyBehaviour<T> : NetworkBehaviour`).
* **Allowed:** RPC methods using generic parameter types from their enclosing class (e.g., `void MyRpc(T arg)`).
* **Allowed:** Closed generic types (e.g., `MyStruct<int>`) as RPC arguments.
* **Disallowed:** RPC methods declaring their own generic parameters (e.g., `void MyRpc<T>(T arg)`), because the Weaver needs exact types at compile-time to generate serialization code.

---

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1202.cs' Name:'mirage1202-triggering' }}}

---

## How to Resolve
* Remove generic parameters from the RPC method signature. Use class-level generic parameters or closed generic types instead.
* Change the return type to `void`, `UniTask`, or `UniTask<T>`.

{{{ Path:'Snippets/Analyzers/Mirage1202.cs' Name:'mirage1202-resolved' }}}
