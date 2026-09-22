# MIRAGE1302: Unserialized Member Warning

## When this appears

An instance field or property in a `[NetworkMessage]` type appears to hold message data but is omitted by default generated serialization.

Generated serialization includes eligible public instance fields, including inherited fields. It omits ordinary properties and fields declared private, internal or protected.

This warning excludes static members, fields explicitly marked `[System.NonSerialized]` or `[WeaverIgnore]`, and compiler-generated backing fields. Types using custom or manual serialization are also excluded: their serializers determine which members are transmitted.

The current Weaver **does traverse** `protected internal` and `private protected` fields. Do not rely on those modifiers to keep state local; see the [MIRAGE1301 visibility caveat](./MIRAGE1301.md).

{{{ Path:'Snippets/Analyzers/Mirage1302.cs' Name:'mirage1302-triggering' }}}

## How to fix

Use public fields for transmitted data, or write a compatible custom reader/writer pair. For local state, explicitly ignore the field or suppress the warning on the declaration. Making instance state static changes its meaning and is not a general fix.

{{{ Path:'Snippets/Analyzers/Mirage1302.cs' Name:'mirage1302-resolved' }}}
