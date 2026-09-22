# MIRAGE1203: Pass-by-Reference Modifiers in RPCs

## When this appears

A `[ServerRpc]` or `[ClientRpc]` parameter uses `ref`, `out`, or readonly `in`. RPCs transfer serialized values and cannot share the caller's variable across the network. The Weaver does not treat `in` as an ordinary value parameter.

{{{ Path:'Snippets/Analyzers/Mirage1203.cs' Name:'mirage1203-triggering' }}}

## How to fix

Pass arguments by value. To publish updated state, use a synchronized field such as `[SyncVar]`:

{{{ Path:'Snippets/Analyzers/Mirage1203.cs' Name:'mirage1203-recommended' }}}

To return a result, use `UniTask<T>` with serializable `T` on a ServerRpc or Owner/Player ClientRpc. Observers ClientRpcs cannot return results; see [MIRAGE1205](./MIRAGE1205.md).

Validate gameplay inputs and permissions on the server before applying changes in either example.

{{{ Path:'Snippets/Analyzers/Mirage1203.cs' Name:'mirage1203-alternative' }}}
