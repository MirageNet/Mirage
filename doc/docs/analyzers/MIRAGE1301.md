# MIRAGE1301: Field Type Serialization Validation

## The Problem
A value that Mirage actually serializes has no usable reader or writer. This includes transmitted RPC parameters, `UniTask<T>` RPC results, and included network-message fields, including their nested and inherited fields.

Mirage discovers built-in and custom serializers before attempting to generate a missing serializer. A custom serializer can support a type that default generation rejects, such as an interface or abstract class. Field or parameter attributes can select a different serialization path, including a maximum-length serializer.

Default generation supports concrete structs and classes whose included fields can be serialized. Ordinary classes need a **public parameterless constructor** for the generated reader. Closed generic types have generation paths; an open generic definition is not a concrete message. `NetworkBehaviour` references and `ScriptableObject` subclasses use special serialization or construction paths.

Use public instance fields for generated message data. Ordinary properties are not traversed. Static, private, internal and protected fields, and fields marked `[System.NonSerialized]` or `[WeaverIgnore]`, are skipped. An unsupported type on a skipped member does not trigger this rule. RPC `INetworkPlayer` routing/sender parameters are also not serialized payloads.

**Current Weaver caveat:** its field filter also traverses `protected internal` and `private protected` fields. This is not a supported visibility contract and may produce inaccessible generated field access. Do not assume these fields remain local or omit them from analysis solely because they are non-public. Use public fields for transmitted data, or an explicit ignore attribute for local state.

Types marked `[WeaverWriteAsGeneric]` use manually assigned `Writer<T>` and `Reader<T>` delegates. Their presence cannot prove that those delegates will be initialized correctly at runtime; this rule must not reject the type merely because default generation is unavailable.

---

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1301.cs' Name:'mirage1301-triggering' }}}

---

## How to Resolve

Replace the unsupported transmitted value with a supported type or identifier, exclude state that is intentionally local, or provide compatible custom reader and writer extension methods. For an ordinary class using generated serialization, supply a public parameterless constructor and make every included field serializable. Adding `[NetworkMessage]` alone does not make an unsupported field type serializable.

{{{ Path:'Snippets/Analyzers/Mirage1301.cs' Name:'mirage1301-resolved' }}}
