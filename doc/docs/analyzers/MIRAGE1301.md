# MIRAGE1301: Field Type Serialization Validation

## When this appears

An RPC parameter, `UniTask<T>` RPC result, or serialized message field is missing a reader or writer.

The sender needs a writer to turn the value into bytes, and the receiver needs a reader to read it back. Without both, the RPC or message cannot carry that value correctly.

{{{ Path:'Snippets/Analyzers/Mirage1301.cs' Name:'mirage1301-triggering' }}}

## How to fix

Use a supported type or send an identifier instead. For local fields, add `[System.NonSerialized]` or `[WeaverIgnore]`. If you need to send the type itself, provide a compatible custom reader/writer pair.

Generated classes and structs need serializers for every field they send, including nested and inherited fields. Ordinary classes also need a **public parameterless constructor**. Adding `[NetworkMessage]` alone does not make unsupported fields serializable.

{{{ Path:'Snippets/Analyzers/Mirage1301.cs' Name:'mirage1301-resolved' }}}

## Serialization details

Built-in and custom serializers are used before Weaver tries to generate one. A custom serializer can support interfaces or abstract classes. Field and parameter attributes can select a different serializer, including maximum-length serialization.

Closed generic types can use generated serialization; open generic definitions are not concrete messages. `NetworkBehaviour` references and `ScriptableObject` subclasses use special serialization or construction paths.

`[WeaverWriteAsGeneric]` uses manually assigned `Writer<T>`/`Reader<T>` delegates. It allows types that cannot use generated serialization, but you must initialize those delegates before use.

Default generation skips ordinary properties and static, private, internal or protected fields, plus fields marked `[System.NonSerialized]` or `[WeaverIgnore]`. Unsupported types on skipped members do not trigger this rule. RPC `INetworkPlayer` routing/sender parameters are also excluded.

**Visibility caveat:** Weaver currently includes `protected internal` and `private protected` fields, which can produce code that cannot access them. Do not rely on this behavior: use public fields for transmitted data and explicit ignore attributes for local state.
