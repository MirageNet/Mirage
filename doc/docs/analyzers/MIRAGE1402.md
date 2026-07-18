# MIRAGE1402: Missing base Call in OnSerialize/OnDeserialize

## The Problem
An overridden `OnSerialize` or `OnDeserialize` method in a class derived from a stateful `NetworkBehaviour` does not call its base implementation. This prevents base properties and `SyncVars` from synchronizing, leading to out-of-sync states between the server and clients.

---

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1402.cs' Name:'mirage1402-triggering' }}}

---

## How to Resolve
Call the base method within the override and combine its return value with the derived status.

{{{ Path:'Snippets/Analyzers/Mirage1402.cs' Name:'mirage1402-resolved' }}}
