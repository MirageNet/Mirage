# MIRAGE1304: Non-Serializable MonoBehaviour Parameter

## When this appears

A serialized RPC parameter or message field refers to a `MonoBehaviour` that has no usable serializer.

An ordinary component reference only identifies an object in the local Unity process. Generated serialization cannot tell the receiver which component to use, so Weaver cannot generate a serializer for it.

{{{ Path:'Snippets/Analyzers/Mirage1304.cs' Name:'mirage1304-triggering' }}}

## How to fix

For a networked component, inherit from `NetworkBehaviour`. Mirage sends its network ID and component index; public field values are not included.

The component must belong to a spawned `NetworkIdentity` that the receiver can find, with the same network component layout. Changing the base class alone does not set this up.

{{{ Path:'Snippets/Analyzers/Mirage1304.cs' Name:'mirage1304-recommended' }}}

Otherwise, send a serializable identifier and look up the component when the message arrives. Both peers must agree on the mapping; a local Unity instance ID alone is not enough. A custom reader/writer pair can perform this lookup.

{{{ Path:'Snippets/Analyzers/Mirage1304.cs' Name:'mirage1304-alternative' }}}

## Exceptions

`NetworkIdentity` has a built-in serializer, even though it does not inherit `NetworkBehaviour`.

A custom serializer is used before Weaver checks whether it can generate one. Manual serialization through `[WeaverWriteAsGeneric]` is also possible; see [MIRAGE1301](./MIRAGE1301.md).

Ignored message fields and ordinary properties are not serialized by default and do not trigger this rule.
