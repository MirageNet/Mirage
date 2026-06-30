# MIRAGE1402: Missing base Call in OnSerialize/OnDeserialize

## The Problem
Overriding `OnSerialize` or `OnDeserialize` in a derived `NetworkBehaviour` class without calling `base.OnSerialize` or `base.OnDeserialize`.

Derived classes that inherit from another `NetworkBehaviour` which has its own synchronized state must call the base implementation. Failing to call the base method prevents the base class's properties and SyncVars from being serialized or deserialized, leading to out-of-sync states between the server and clients.

---

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1402.cs' Name:'mirage1402-triggering' }}}

---

## How to Resolve

Add the call to `base.OnSerialize` or `base.OnDeserialize` inside the overridden method and combine its return value with the derived class's serialization status.

{{{ Path:'Snippets/Analyzers/Mirage1402.cs' Name:'mirage1402-resolved' }}}
