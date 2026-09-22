# MIRAGE1005: Readonly SyncVar Field

## When this appears

A `[SyncVar]` field is `readonly`.

Receiving a SyncVar update assigns a value after the object has been constructed. That conflicts with C#'s restriction on assigning `readonly` fields.

{{{ Path:'Snippets/Analyzers/Mirage1005.cs' Name:'mirage1005-triggering' }}}

## How to fix

Remove `readonly` from an ordinary SyncVar field.

{{{ Path:'Snippets/Analyzers/Mirage1005.cs' Name:'mirage1005-resolved' }}}

If the field implements `ISyncObject`, remove `[SyncVar]` instead and keep the stable `readonly` reference. SyncObjects synchronize themselves; see [MIRAGE1003](./MIRAGE1003.md).

This rule also applies to `initialOnly` SyncVars: the initial received value still needs to be assigned after construction. For reference types, `readonly` restricts field assignment, not changes to the referenced object's contents.

This check comes from the analyzer. Weaver does not explicitly reject `readonly` before emitting writes; successful weaving does not make this a supported pattern.
