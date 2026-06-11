# ZigZag Encode

To encode a value using [ZigZag Encoding](https://gist.github.com/mfuerstenau/ba870a29e16536fdbaba) you can use the [ZigZagEncodeAttribute](/docs/reference/Mirage.Serialization/ZigZagEncodeAttribute)

This will cause negative values to be encoded as positive so that the sign bit is not lost when packing.

This works best with [VarIntBlocks](/docs/guides/bit-packing/var-int-blocks) but also works with [BitCount](/docs/guides/bit-packing/bit-count).

This attribute can not be used on the same field as [BitCountFromRange](/docs/guides/bit-packing/bit-count-from-range), this is because `BitCountFromRange` already ensures negative values are packed correctly.

:::note
The sign of a value will take up 1 bit, so if the value is in the range -+100 it will need a bit count of 8
:::

### Use cases

- A value that can be negative or positive

### Supported Types

- Byte
- Short
- Int
- Long
- Enum

### Example 1

A modifier that can be added to a character value to increase or decrease it

{{{ Path:'Snippets/BitPacking/ZigZagEncodeSnippets.cs' Name:'zig-zag-encode-example-1' }}}

`Range = 200` so bit count is 8, causing the real range to be -128 to 127

`modifier = 57` will serialize to `0111_0010`

`modifier = -57` will serialize to `0111_0001`

`modifier = -110` will serialize to `1101_1011`, even tho -110 is out of range there is enough range because bit count rounds up.

`modifier = 130` *(out of range)* will serialize to `0000_0100`

### Generated Code

Source:
{{{ Path:'Snippets/BitPacking/ZigZagEncodeSnippets.cs' Name:'zig-zag-encode-generated-source' }}}

Generated:
{{{ Path:'Snippets/BitPacking/ZigZagEncodeSnippets.cs' Name:'zig-zag-encode-generated-code' }}}

*last updated for Mirage v101.8.0*