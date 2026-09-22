# MIRAGE1301: Field Type Serialization Validation

## When this appears

A transmitted RPC parameter, `UniTask<T>` RPC result, or included message field lacks a usable reader or writer. This includes nested and inherited serialized fields.

Serialization depends on the selected path:

- Built-in and custom serializers take precedence over generation. Custom serializers can support interfaces or abstract classes. Field and parameter attributes can select another path, including maximum-length serialization.
- Generated concrete structs and classes require serializable included fields. Ordinary classes also need a **public parameterless constructor**. Closed generic types have generation paths; open generic definitions are not concrete messages.
- `NetworkBehaviour` references and `ScriptableObject` subclasses use special serialization or construction paths.
- `[WeaverWriteAsGeneric]` uses manually assigned `Writer<T>`/`Reader<T>` delegates. Default-generation restrictions do not invalidate this path, but the attribute does not guarantee runtime initialization.

Default generation skips ordinary properties and static, private, internal or protected fields, plus fields marked `[System.NonSerialized]` or `[WeaverIgnore]`. Unsupported types on skipped members do not trigger this rule. RPC `INetworkPlayer` routing/sender parameters are also excluded.

**Weaver caveat:** `protected internal` and `private protected` fields are currently traversed and can cause inaccessible generated access. This is unsupported behavior: use public fields for transmitted data or explicit ignore attributes for local state.

{{{ Path:'Snippets/Analyzers/Mirage1301.cs' Name:'mirage1301-triggering' }}}

## How to fix

Use a supported type or identifier, explicitly exclude local fields, or provide compatible custom reader/writer extensions. For generated classes, meet the field and constructor requirements above. Adding `[NetworkMessage]` alone does not make unsupported fields serializable.

{{{ Path:'Snippets/Analyzers/Mirage1301.cs' Name:'mirage1301-resolved' }}}
