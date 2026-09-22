# MIRAGE1402: Missing base Call in OnSerialize/OnDeserialize

## The Problem
An override of `NetworkBehaviour.OnSerialize` or `OnDeserialize` that omits its base call can skip generated SyncVar serialization, SyncObjects, or a base class's custom data. This includes SyncVars declared on the overriding class: the default methods dispatch to the generated `SerializeSyncVars` and `DeserializeSyncVars` implementations. Ordinary properties are not automatically synchronized.

This is a warning about preserving the serialization contract. An intentional complete replacement can suppress it after verifying both writing and reading; a method with the same name that does not override these Mirage methods is outside the rule.

---

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1402.cs' Name:'mirage1402-triggering' }}}

---

## How to Resolve
Call the base method exactly once on each serialization path, then write and read custom fields in the same order. Pair every additional write with a matching read. Preserve the base result when only conditionally writing custom data; when custom data is always written, returning `true` is sufficient.

Do not write a `[SyncVar]` again as custom data: the base call already handles it. The example uses an ordinary custom field and marks the behaviour dirty when it changes. Returning `true` from `OnSerialize` does not itself schedule synchronization; dirty state and `SyncSettings` determine when Mirage calls it.

{{{ Path:'Snippets/Analyzers/Mirage1402.cs' Name:'mirage1402-resolved' }}}
