# MIRAGE1005: Readonly SyncVar Field

## The Problem
A field marked with `[SyncVar]` is `readonly`.

SyncVars must be synchronized from server to clients. The Weaver generates setters on these fields to track changes and trigger updates. Since `readonly` fields are immutable at runtime, the Weaver cannot wrap them and the server cannot update their values.

---

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1005.cs' Name:'mirage1005-triggering' }}}

---

## How to Resolve

Remove the `readonly` modifier from the field.

{{{ Path:'Snippets/Analyzers/Mirage1005.cs' Name:'mirage1005-resolved' }}}
