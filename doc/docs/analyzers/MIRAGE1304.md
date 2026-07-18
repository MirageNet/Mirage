# MIRAGE1304: Non-Serializable MonoBehaviour Parameter

## The Problem
An RPC parameter or a `[NetworkMessage]` field is a `MonoBehaviour` type (or a subclass of it) that does not inherit from `NetworkBehaviour`.

In Unity, a basic `MonoBehaviour` represents a local component attached to a GameObject. Because it lacks a network identity (`NetworkIdentity`), Mirage cannot identify which instance of the component to refer to across the network. Consequently, the Weaver cannot automatically generate serialization code for `MonoBehaviour` references, resulting in a compile-time weaving error.

---

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1304.cs' Name:'mirage1304-triggering' }}}

---

## How to Resolve

### Recommended Fix: Inherit from NetworkBehaviour
Ensure the component type inherits from `NetworkBehaviour` instead of `MonoBehaviour`. Mirage is able to serialize components that inherit from `NetworkBehaviour` by writing their parent `NetworkIdentity` and the component's index.

{{{ Path:'Snippets/Analyzers/Mirage1304.cs' Name:'mirage1304-recommended' }}}

---

### Alternative Solution: Use a serializable identifier
If the component itself is purely local and cannot be networked, pass a serializable identifier (like a unique ID, parent `NetworkIdentity`, or string) instead of the component object itself.


{{{ Path:'Snippets/Analyzers/Mirage1304.cs' Name:'mirage1304-alternative' }}}

