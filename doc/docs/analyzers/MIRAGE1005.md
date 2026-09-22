# MIRAGE1005: Readonly SyncVar Field

## The Problem
A field marked with `[SyncVar]` is `readonly`.

This analyzer policy requires writable storage for received synchronized state. C# `readonly` restricts field assignment to initialization, while Mirage's generated deserializer must assign the field after construction. The policy also applies to `initialOnly` SyncVars, whose initial state is received after construction.

The current Weaver does not explicitly reject the readonly flag before emitting field writes. A missing Weaver error is therefore not evidence that readonly SyncVars are supported. For a reference-type field, `readonly` also does not make the referenced object's contents immutable.

---

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1005.cs' Name:'mirage1005-triggering' }}}

---

## How to Resolve

Remove the `readonly` modifier from the field.

If the field implements `ISyncObject`, it should not be a SyncVar at all. Remove `[SyncVar]` instead and retain the stable `readonly` SyncObject reference described in [MIRAGE1003](./MIRAGE1003.md). That fixes the unsupported combination without introducing a reassignment risk.

{{{ Path:'Snippets/Analyzers/Mirage1005.cs' Name:'mirage1005-resolved' }}}
