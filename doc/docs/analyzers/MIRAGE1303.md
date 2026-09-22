# MIRAGE1303: Mismatched Custom Serialization Methods

## When this appears

A custom writer has no matching custom reader, or a custom reader has no matching custom writer. The pair must use the same type and serialization mode.

The reader must decode what the writer sends. If they use different encodings, the receiver can read incorrect values or fail to read the message.

{{{ Path:'Snippets/Analyzers/Mirage1303.cs' Name:'mirage1303-triggering' }}}

## How to fix

Add the missing reader or writer and use the same encoding in both. Here, `WriteInt32` needs `ReadInt32`; a generated reader uses a packed integer instead.

{{{ Path:'Snippets/Analyzers/Mirage1303.cs' Name:'mirage1303-resolved' }}}

## Custom serializer pairs

Use public extension methods in accessible static classes:

| Mode | Writer | Reader |
|---|---|---|
| Standard | `public static void Write(this NetworkWriter writer, MyType value)` | `public static MyType Read(this NetworkReader reader)` |
| Maximum-length | `public static void Write(this NetworkWriter writer, MyType value, int maxLength)` | `public static MyType Read(this NetworkReader reader, int maxLength)` |

Names and containing classes need not match. Weaver searches the current assembly, Mirage and referenced assemblies; a pair can span assemblies. `[WeaverIgnore]` excludes methods. Generic extensions need `[WeaverSerializeCollection]` registration.

The two modes are separate registrations. `[MaxLength(n)]` selects maximum-length mode and supplies a **limit**, not the actual length. Both methods must enforce the limit and encode/decode any actual length needed by the reader.

Weaver finds readers and writers separately. A built-in or generated method may supply the other half, so a missing custom pair does not always stop the build. Matching method signatures alone do not guarantee matching encodings.

If you intentionally use a built-in or generated method for the other half, verify that its encoding matches before suppressing this rule. If no reader or no writer is available, also resolve [MIRAGE1301](./MIRAGE1301.md).
