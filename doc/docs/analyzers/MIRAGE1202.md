# MIRAGE1202: Pass-by-Reference Modifiers in RPCs

## The Problem
An RPC method contains parameters with `ref` or `out` parameter modifiers.

RPCs (Remote Procedure Calls) serialize arguments and send them over the network. Pass-by-reference modifiers (`ref` or `out`) imply that the method can modify the argument and pass the changes back to the caller in-place, which is impossible over a one-way network serialization boundary.

---

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1202.cs' Name:'mirage1202-triggering' }}}

---

## How to Resolve

Pass parameters by value. If you need to communicate updated state back to the caller, either use an asynchronous RPC with a `UniTask<T>` return value or update a synchronized property (such as a `[SyncVar]`).

{{{ Path:'Snippets/Analyzers/Mirage1202.cs' Name:'mirage1202-resolved' }}}
