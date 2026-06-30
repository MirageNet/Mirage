# MIRAGE1202: RPC Signature Error

## The Problem
A method decorated with `[ServerRpc]` or `[ClientRpc]` violates remote procedure call rules:
1. **Generic Methods:** The RPC method *itself* cannot have generic parameters (e.g. `void MyRpc<T>()`).
2. **Invalid Return Type:** The method must return `void`, `UniTask`, or `UniTask<T>`. Returning standard tasks, custom classes, or primitive values directly (without a wrapper) is invalid.

### What is Allowed vs. Disallowed for Generics?
* **Allowed:** Defining the `NetworkBehaviour` as a generic class (e.g., `class MyBehaviour<T> : NetworkBehaviour`).
* **Allowed:** RPC methods accepting generic parameter types defined by the enclosing generic class (e.g., `void MyRpc(T arg)`).
* **Allowed:** Using closed generic types (like `MyStruct<int>`) as RPC arguments.
* **Disallowed:** The RPC method itself having generic parameters (e.g., `void MyRpc<T>(T arg)`). Remote procedure calls must serialize their arguments and return values across the network, and the Mirage Weaver must know the exact type definitions at compile-time to generate serialization code.

---

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1202.cs' Name:'mirage1202-triggering' }}}

---

## How to Resolve

1. Ensure the method itself is non-generic (remove any `<T>` generic parameter definitions from the method signature). If you need generic behaviors, consider generic class-level parameters or concrete/closed generic types.
2. Ensure the return type is `void` or a valid async task wrapper like `UniTask` (or `UniTask<T>` for asynchronous RPCs).

{{{ Path:'Snippets/Analyzers/Mirage1202.cs' Name:'mirage1202-resolved' }}}
