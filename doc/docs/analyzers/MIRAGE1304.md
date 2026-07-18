# MIRAGE1304: Non-Serializable MonoBehaviour Parameter

## The Problem
An RPC parameter or `[NetworkMessage]` field is a `MonoBehaviour` type that does not inherit from `NetworkBehaviour`.

Because basic `MonoBehaviour` components lack network identities, Mirage cannot identify them across the network. The Weaver cannot generate serialization code for these component references, causing a compile error.

---

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1304.cs' Name:'mirage1304-triggering' }}}

---

## How to Resolve

### Recommended Fix: Inherit from NetworkBehaviour
Change the component to inherit from `NetworkBehaviour` instead of `MonoBehaviour`. Mirage can serialize `NetworkBehaviour` components by sending their `NetworkIdentity` and component index.

{{{ Path:'Snippets/Analyzers/Mirage1304.cs' Name:'mirage1304-recommended' }}}

---

### Alternative Solution: Use a serializable identifier
If the component cannot be networked, send a serializable identifier (such as an ID or string) instead of the component itself.

{{{ Path:'Snippets/Analyzers/Mirage1304.cs' Name:'mirage1304-alternative' }}}

