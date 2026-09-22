# MIRAGE1302: Unserialized Member Warning

## The Problem
An instance field or property in a `[NetworkMessage]` type appears to hold message data but is omitted by default generated serialization.

The supported generated layout uses eligible public instance fields, including inherited fields. Ordinary properties and fields declared private, internal or protected are omitted. Static members and fields explicitly marked `[System.NonSerialized]` or `[WeaverIgnore]` are intentional exclusions and should not produce this warning. Compiler-generated backing fields should not produce duplicate warnings.

The current Weaver does not omit the combined `protected internal` and `private protected` accessibilities; see the [field-filter caveat in MIRAGE1301](./MIRAGE1301.md). This omission warning must not claim those fields are local-only.

A custom serializer can explicitly serialize properties or otherwise choose its own layout. This rule should skip types using custom or manual serialization rather than infer what those serializers transmit from member visibility.

---

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1302.cs' Name:'mirage1302-triggering' }}}

---

## How to Resolve
Use public fields for data that should participate in generated serialization, or write a compatible custom reader/writer pair. For intentional local state, mark a field with an ignore attribute or suppress the warning on the declaration. Changing instance state to static changes its meaning and is not a general fix.

{{{ Path:'Snippets/Analyzers/Mirage1302.cs' Name:'mirage1302-resolved' }}}
