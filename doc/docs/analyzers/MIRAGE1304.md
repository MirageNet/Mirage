# MIRAGE1304: Non-Serializable MonoBehaviour Parameter

## The Problem
A transmitted RPC parameter or included network-message field refers to an ordinary `MonoBehaviour` type for which no usable serializer is available.

Default generation does not serialize arbitrary Unity component references. `NetworkBehaviour` subclasses use Mirage's network-reference serializer, and `NetworkIdentity` has a built-in serializer even though it is not a `NetworkBehaviour`. A custom serializer for another component type takes precedence over default-generation restrictions; manual serialization through `[WeaverWriteAsGeneric]` is also a distinct path.

Only inspect values that are actually serialized. Ignored message fields and ordinary message properties do not require a serializer.

---

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1304.cs' Name:'mirage1304-triggering' }}}

---

## How to Resolve

### Recommended Fix: Inherit from NetworkBehaviour
If this component belongs to a networked object, inherit from `NetworkBehaviour`. Mirage sends its network ID and component index, rather than the component's public field values. The instance must belong to a spawned `NetworkIdentity` that the receiver can resolve, with a matching component layout. Changing the base class alone does not create or spawn that identity.

{{{ Path:'Snippets/Analyzers/Mirage1304.cs' Name:'mirage1304-recommended' }}}

---

### Alternative Solution: Use a serializable identifier
If the component cannot be networked, send a serializable identifier (such as an ID or string) instead of the component itself.

{{{ Path:'Snippets/Analyzers/Mirage1304.cs' Name:'mirage1304-alternative' }}}

A custom reader/writer pair can also encode an application-specific identifier and resolve the existing component on receipt. Both peers must agree on that mapping; a local Unity instance ID alone does not establish it.
