# MIRAGE1303: Mismatched Custom Serialization Methods

## The Problem
A discovered custom writer or reader has no custom counterpart for the same data type and serialization mode. Requiring both custom methods is an **analyzer policy** intended to prevent incompatible wire formats.

The Weaver resolves readers and writers independently. A missing custom counterpart can fall back to a built-in or generated method, so the absence of a pair does not necessarily cause a Weaver error. That fallback may use a different encoding. Even matching signatures do not prove that two method bodies agree.

Mirage recognizes these extension-method shapes; the method names and containing classes do not need to match:

### Standard Signatures
- **Writer:** `public static void WriteMyType(this NetworkWriter writer, MyType value)`
- **Reader:** `public static MyType ReadMyType(this NetworkReader reader)`

### Maximum-length Signatures
- **Writer:** `public static void WriteMyType(this NetworkWriter writer, MyType value, int maxLength)`
- **Reader:** `public static MyType ReadMyType(this NetworkReader reader, int maxLength)`

Standard and maximum-length serializers are separate registrations. `[MaxLength(n)]` selects the latter and passes `n` as a limit, not as the actual length of the value. The custom methods must enforce the limit and encode any actual length the reader needs.

Discovery searches static classes in the current assembly, Mirage and referenced assemblies. Use public accessible extension methods. `[WeaverIgnore]` excludes a method; ordinary generic extension methods are not candidates unless they use the supported `[WeaverSerializeCollection]` registration path. Pairing must consider discovered methods across classes and assemblies, rather than require both methods in one source file.

---

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1303.cs' Name:'mirage1303-triggering' }}}

---

## How to Resolve
Define the missing custom counterpart with the same target type and mode, and make its encoding match. In this example the writer uses a fixed 32-bit integer, so the reader must use `ReadInt32`; the generated reader would use a packed integer instead.

If a generated counterpart is deliberately compatible, suppress this policy diagnostic after verifying the wire format. If no counterpart can be discovered or generated for a required direction, that is also a serialization-generation failure.

{{{ Path:'Snippets/Analyzers/Mirage1303.cs' Name:'mirage1303-resolved' }}}
