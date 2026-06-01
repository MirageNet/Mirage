# Bit Count

The bit count of Integer based fields can be set using the [BitCountAttribute](/docs/reference/Mirage.Serialization/BitCountAttribute).

This will truncate the bits so that only the small bits are sent. There is no range checking for values using BitCount, so the value that is too big or negative will not be unpacked correctly

This means that `BitCount` should not be used with values that can be negative because this data will be lost. If you do need to send negative values then use [ZigZagEncode](/docs/guides/bit-packing/zig-zag-encode) or [BitCountFromRange](/docs/guides/bit-packing/bit-count-from-range)

### Use cases

- A Value with a maximum value
- An index in an array of known size
    - eg array with 10 elements, the index can be sent as 4 bits
- A Random int hash where you only need to send 16 bits

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

Health which is between 0 and 100

{{{ Path:'Snippets/BitPacking/BitCountSnippets.cs' Name:'bit-count-example-1' }}}

`BitCount = 7` so max value of Health is `127`

`health = 57` will serialize to `011_1001`

`health = -1` *(out of range)* will serialize to `111_1111`

`health = 130` *(out of range)* will serialize to `000_0010`


### Example 2

Weapon index in a list of 6 weapons
{{{ Path:'Snippets/BitPacking/BitCountSnippets.cs' Name:'bit-count-example-2' }}}

`BitCount = 3` so max value of Health is 7

`WeaponIndex = 5` will serialize to `101`


### Generated Code

Source:
{{{ Path:'Snippets/BitPacking/BitCountSnippets.cs' Name:'bit-count-generated-source' }}}

Generated:
{{{ Path:'Snippets/BitPacking/BitCountSnippets.cs' Name:'bit-count-generated-code' }}}

*Last updated for Mirage v101.8.0.*