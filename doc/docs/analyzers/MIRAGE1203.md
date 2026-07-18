# MIRAGE1203: Pass-by-Reference Modifiers in RPCs

## The Problem
An RPC method contains parameters with `ref`, `out`, or `in` modifiers.

Because RPC arguments must be serialized across the network, pass-by-reference modifiers are not supported.

---

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1203.cs' Name:'mirage1203-triggering' }}}

---

## How to Resolve

### Recommended Fix: Pass by value
Pass parameters by value. This is the standard way to transfer data in RPCs.

{{{ Path:'Snippets/Analyzers/Mirage1203.cs' Name:'mirage1203-recommended' }}}

---

### Alternative Solutions
To return updated state to the caller:
- Use an asynchronous RPC with a `UniTask<T>` return value.
- Update a synchronized field like `[SyncVar]`.

{{{ Path:'Snippets/Analyzers/Mirage1203.cs' Name:'mirage1203-alternative' }}}