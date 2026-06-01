# Float Pack

A float value can be compressed using [FloatPackAttribute](/docs/reference/Mirage.Serialization/FloatPackAttribute)

The float value will be quantized and turned into an integer with at least the resolution that is given in the attribute.

The real resolution used is calculated from the `bitcount` required to pack the value. For example, if the max is `100`, and the resolution is `0.1f`, then there are `2000` discrete values are needed to pack this. This requires 11 bits. 11 bits allow for `2047` discrete values so the real resolution used will be `0.0977f`.

Values are packed so that 0 will unpack are 0, and other values are rounded to the nearest int so that rounding errors stay as low as possible.

Values are clamped so values out of range will be packed as min/max values instead

### Use cases

- A Value with a maximum value

### Supported Types

- Float

### Example 1

Health which is between 0 and 100

{{{ Path:'Snippets/BitPacking/FloatPackSnippets.cs' Name:'float-pack-example-1' }}}

`Max = 100`, `resolution = 0.02f` so bit count is 14

`health = 57.2f` will serialize to `01_0010_0100_1101` and deserialize to `57.197f`

`health = -13.5f` will serialize to `11_1011_1010_1110` and deserialize to `-13.503f`

`health = 120f` will be clamped to `100f`


### Example 2

A Percent that where you only want to send 8 bits

{{{ Path:'Snippets/BitPacking/FloatPackSnippets.cs' Name:'float-pack-example-2' }}}

`Max = 1f`, `bitCount = 8` so resolution will be `0.00787f`

### Generated Code

Source:
{{{ Path:'Snippets/BitPacking/FloatPackSnippets.cs' Name:'float-pack-generated-source' }}}

Generated:
{{{ Path:'Snippets/BitPacking/FloatPackSnippets.cs' Name:'float-pack-generated-code' }}}

*last updated for Mirage v101.8.0*