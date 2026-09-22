# MIRAGE1203: Pass-by-Reference Modifiers in RPCs

## The Problem
An RPC method contains parameters with `ref`, `out`, or `in` modifiers.

Because RPC arguments must be serialized across the network, pass-by-reference modifiers are not supported. This includes readonly `in` parameters; they are not treated as ordinary value parameters by the Weaver.

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
- Use a ServerRpc or Owner/Player ClientRpc with a serializable `UniTask<T>` result. An Observers ClientRpc cannot return a result; see [MIRAGE1205](./MIRAGE1205.md).
- Update a synchronized field like `[SyncVar]`.

The example below returns a calculated value. Production gameplay RPCs also need server-side input and permission checks; changing the parameter modifier does not supply those checks.

{{{ Path:'Snippets/Analyzers/Mirage1203.cs' Name:'mirage1203-alternative' }}}
