# Bit Count From Range

The bit count of Integer based fields can be set using [BitCountFromRangeAttribute](/docs/reference/Mirage.Serialization/BitCountFromRangeAttribute) It will use the given range to calculate the required bit count. This works in a similar way to [BitCount](/docs/guides/bit-packing/bit-count)

The min value is subtracted from the value before it is written and added back after it is read. This will shift all written values into the positive range for writing so that the sign bit is not lost.

This will truncate the bits so that only the small bits are sent. There is no range checking for values using BitCount, so the value that is too big or too small will not be unpacked correctly.

Bit Count is calculated using `bitCount = 1 + Floor(Log2(max - min))`, so `min = -100`, `max = 100` results in `bit count = 8`

Values are written using `Write(value - min, bitCount)` and read using `value = Read(bitCount) + min`

### Use cases

- A Value with a minimum and maximum value

### Supported Types

- Byte
- Short
- UShort
- Int
- Uint
- Long
- ULong
- Enum

### Example 1

A modifier that can add to a character value to increase or decrease it

{{{ Path:'Snippets/BitPacking/BitCountFromRangeSnippets.cs' Name:'bit-count-from-range-example-1' }}}

`Range = 200` so bit count is 8, causing the real range to be -100 to 155

`modifier = 57` will serialize to `1001_1101`

`modifier = -57` will serialize to `0010_1011`

`modifier = -110` *(out of range)*  will serialize to `1111_0110`

`modifier = 130` will serialize to `1110_0110`, even tho 130 is out of range there is enough range because bit count rounds up.

`modifier = 170` *(out of range)* will serialize to `0000_1110`


### Example 2

A Direction enum to say which way a model is facing

{{{ Path:'Snippets/BitPacking/BitCountFromRangeSnippets.cs' Name:'bit-count-from-range-example-2' }}}

`Range = 3` so bit count is `2`, causing the real range to be -1 to 2

`direction = -1` will serialize to `00`

`direction = 1` will serialize to `10`


### Generated Code

Source:
{{{ Path:'Snippets/BitPacking/BitCountFromRangeSnippets.cs' Name:'bit-count-from-range-generated-source' }}}

Generated:
{{{ Path:'Snippets/BitPacking/BitCountFromRangeSnippets.cs' Name:'bit-count-from-range-generated-code' }}}

*last updated for Mirage v101.8.0*
