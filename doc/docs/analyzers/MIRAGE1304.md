# MIRAGE1304: Non-Serializable MonoBehaviour Parameter

## When this appears

A transmitted RPC parameter or included message field refers to an ordinary `MonoBehaviour` without a usable serializer. Ignored message fields and ordinary properties are outside this rule.

Default generation does not serialize arbitrary component references. Supported alternatives include:

- `NetworkBehaviour` subclasses, using Mirage's network-reference serializer.
- `NetworkIdentity`, which has a built-in serializer despite not inheriting `NetworkBehaviour`.
- A custom serializer, selected before default-generation restrictions, or manual serialization through `[WeaverWriteAsGeneric]` ([MIRAGE1301](./MIRAGE1301.md)).

{{{ Path:'Snippets/Analyzers/Mirage1304.cs' Name:'mirage1304-triggering' }}}

## How to fix

For a networked component, inherit from `NetworkBehaviour`. Mirage sends its network ID and component index; public field values are not included.

The component must belong to a spawned `NetworkIdentity` that the receiver can resolve, with a matching component layout. Changing the base class alone does not establish this.

{{{ Path:'Snippets/Analyzers/Mirage1304.cs' Name:'mirage1304-recommended' }}}

Otherwise, send a serializable identifier and resolve the component on receipt. Both peers must agree on the mapping; a local Unity instance ID alone is insufficient. A custom reader/writer pair can encapsulate this mapping.

{{{ Path:'Snippets/Analyzers/Mirage1304.cs' Name:'mirage1304-alternative' }}}
