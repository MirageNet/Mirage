# MIRAGE1203: Pass-by-Reference Modifiers in RPCs

## The Problem
An RPC method contains parameters with `ref`, `out`, or `in` parameter modifiers.

RPCs (Remote Procedure Calls) serialize arguments and send them over the network. Pass-by-reference modifiers (`ref`, `out`, or `in`) pass values by reference under the hood (which triggers the Weaver's `IsByReference` check). Since RPC arguments must be serialized over a one-way network serialization boundary, pass-by-reference is not supported.

---

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1203.cs' Name:'mirage1203-triggering' }}}

---

## How to Resolve

### Recommended Fix: Pass by value
Pass parameters by value. This is the most direct and performant way to handle data transfer in RPCs.

{{{ Path:'Snippets/Analyzers/Mirage1203.cs' Name:'mirage1203-recommended' }}}

---

### Alternative Solutions
If you need to communicate updated state back to the caller, you can:
1. **Use an asynchronous RPC** with a `UniTask<T>` return value.
2. **Update a synchronized field** (such as a `[SyncVar]`) as shown in the example above.

{{{ Path:'Snippets/Analyzers/Mirage1203.cs' Name:'mirage1203-alternative' }}}