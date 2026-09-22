# MIRAGE1303: Mismatched Custom Serialization Methods

## When this appears

A discovered custom writer or reader lacks a custom counterpart for the same data type and serialization mode. This pairing requirement is **analyzer policy** to reduce incompatible wire formats.

The Weaver resolves each direction independently, so a built-in or generated counterpart can allow weaving to succeed while using an incompatible encoding. Matching signatures alone do not prove matching encodings.

Use public accessible extension methods in static classes:

| Mode | Writer | Reader |
|---|---|---|
| Standard | `public static void Write(this NetworkWriter writer, MyType value)` | `public static MyType Read(this NetworkReader reader)` |
| Maximum-length | `public static void Write(this NetworkWriter writer, MyType value, int maxLength)` | `public static MyType Read(this NetworkReader reader, int maxLength)` |

Names and containing classes need not match. Discovery searches the current assembly, Mirage and referenced assemblies; pairs can span assemblies. `[WeaverIgnore]` excludes methods. Generic extensions require the supported `[WeaverSerializeCollection]` registration path.

The two modes are separate registrations. `[MaxLength(n)]` selects maximum-length mode and supplies a **limit**, not the actual length. Both methods must enforce the limit and encode/decode any actual length needed by the reader.

{{{ Path:'Snippets/Analyzers/Mirage1303.cs' Name:'mirage1303-triggering' }}}

## How to fix

Add the missing counterpart with a matching encoding. Here, `WriteInt32` requires `ReadInt32`; the generated reader uses a packed integer.

Suppress this policy diagnostic if you have verified a compatible generated counterpart. If a required direction has no available serializer, resolve [MIRAGE1301](./MIRAGE1301.md) too.

{{{ Path:'Snippets/Analyzers/Mirage1303.cs' Name:'mirage1303-resolved' }}}
