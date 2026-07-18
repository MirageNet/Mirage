# MIRAGE1005: Readonly SyncVar Field

## The Problem
A field marked with `[SyncVar]` is `readonly`.

SyncVars are synchronized between server and clients. Since `readonly` fields are immutable at runtime, Weaver cannot create setter methods to update the fields.

---

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1005.cs' Name:'mirage1005-triggering' }}}

---

## How to Resolve

Remove the `readonly` modifier from the field.

{{{ Path:'Snippets/Analyzers/Mirage1005.cs' Name:'mirage1005-resolved' }}}
