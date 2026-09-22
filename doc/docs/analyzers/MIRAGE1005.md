# MIRAGE1005: Readonly SyncVar Field

## When this appears

A `[SyncVar]` field is `readonly`. Received state must be assigned after construction, including initial state for `initialOnly` SyncVars.

This writable-storage requirement is an analyzer policy. The Weaver does not explicitly reject `readonly` before emitting writes; successful weaving does not establish support.

For reference types, `readonly` restricts field assignment, not changes to the referenced object's contents.

{{{ Path:'Snippets/Analyzers/Mirage1005.cs' Name:'mirage1005-triggering' }}}

## How to fix

Remove `readonly` from an ordinary SyncVar field.

If the field implements `ISyncObject`, remove `[SyncVar]` instead and keep the stable `readonly` reference. SyncObjects synchronize themselves; see [MIRAGE1003](./MIRAGE1003.md).

{{{ Path:'Snippets/Analyzers/Mirage1005.cs' Name:'mirage1005-resolved' }}}
