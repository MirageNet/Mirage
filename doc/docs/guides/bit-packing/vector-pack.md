# Vector Pack

A Vector2 or Vector3 can be compressed using [Vector2PackAttribute](/docs/reference/Mirage.Serialization/Vector2PackAttribute) or [Vector3PackAttribute](/docs/reference/Mirage.Serialization/Vector3PackAttribute)

These attributes work in the same way as [FloatPack](/docs/guides/bit-packing/float-pack) except on 2 or 3 dimensions instead of 1

### Use cases

- A Vector value with a maximum value

### Supported Types

- Vector2 [Vector2PackAttribute](/docs/reference/Mirage.Serialization/Vector2PackAttribute)
- Vector3 [Vector3PackAttribute](/docs/reference/Mirage.Serialization/Vector3PackAttribute)

### Example 1

A Position in bounds +-100 in all XYZ with 0.05 precision for all axis 

{{{ Path:'Snippets/BitPacking/VectorPackSnippets.cs' Name:'vector-pack-example-1' }}}

### Example 2

A Position in bounds +-100 in all XZ with 0.05 precision, but with +-20 and precision 0.1 in y-axis

{{{ Path:'Snippets/BitPacking/VectorPackSnippets.cs' Name:'vector-pack-example-2' }}}

### Example 3

A position in a 2D map

{{{ Path:'Snippets/BitPacking/VectorPackSnippets.cs' Name:'vector-pack-example-3' }}}

### Generated Code

Source:
{{{ Path:'Snippets/BitPacking/VectorPackSnippets.cs' Name:'vector-pack-generated-source' }}}

Generated:
{{{ Path:'Snippets/BitPacking/VectorPackSnippets.cs' Name:'vector-pack-generated-code' }}}

*last updated for Mirage v101.8.0*