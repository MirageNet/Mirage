# MIRAGE1005: Readonly SyncVar Field

## The Problem
A field decorated with `[SyncVar]` is marked as `readonly`.

SyncVars are synchronized from the server to clients automatically. The Weaver post-processes writes to SyncVar fields by intercepting them to set dirty flags and trigger synchronization. A `readonly` field can only be assigned in constructors or initializers, making it immutable at runtime and preventing the server from updating it or the Weaver from injecting the required setter-interception wrappers properly.

---

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1005.cs' Name:'mirage1005-triggering' }}}

---

## How to Resolve

Remove the `readonly` modifier from the `[SyncVar]` field declaration to allow it to be modified at runtime.

{{{ Path:'Snippets/Analyzers/Mirage1005.cs' Name:'mirage1005-resolved' }}}
