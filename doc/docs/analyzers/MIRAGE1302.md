# MIRAGE1302: Unserialized Member Warning

## When this appears

An instance field or property in a `[NetworkMessage]` type is left out of generated serialization.

The receiver will not get the value you assigned to that member. This can leave part of the message empty or incorrect even though the message is sent successfully.

{{{ Path:'Snippets/Analyzers/Mirage1302.cs' Name:'mirage1302-triggering' }}}

## How to fix

Use public fields for data you want to send, or write a compatible custom reader/writer pair.

For local state, mark the field `[System.NonSerialized]` or `[WeaverIgnore]`, or suppress the warning on the field or property. Making instance state static changes its meaning and is not a general fix.

{{{ Path:'Snippets/Analyzers/Mirage1302.cs' Name:'mirage1302-resolved' }}}

## Which members are skipped

Generated serialization includes public instance fields, including inherited fields. It skips ordinary properties and fields declared private, internal or protected.

This warning excludes static members, explicitly ignored fields and compiler-generated backing fields. Types using custom or manual serialization are also excluded because their serializers decide which members to send.

Weaver currently includes `protected internal` and `private protected` fields. Do not use those modifiers to keep state local; see the [MIRAGE1301 visibility caveat](./MIRAGE1301.md).
