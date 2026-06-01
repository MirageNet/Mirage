# Variable Sized Integer Blocks

:::caution Work In Progress
This page is a work in progress
:::

Packs an integer value based on its size

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

A modifier that can be added to a character value to increase or decrease it

{{{ Path:'Snippets/BitPacking/VarIntBlocksSnippets.cs' Name:'var-int-blocks-example-1' }}}

`Range = 200` so bit count is 8, causing the real range to be -100 to 155

`modifier = 57` will serialize to `1001_1101`

`modifier = -57` will serialize to `0010_1011`

`modifier = -110` *(out of range)*  will serialize to `1111_0110`

`modifier = 130` will serialize to `1110_0110`, even tho 130 is out of range there is enough range because bit count rounds up.

`modifier = 170` *(out of range)* will serialize to `0000_1110`


### Example 2

A Direction enum to say which way a model is facing

{{{ Path:'Snippets/BitPacking/VarIntBlocksSnippets.cs' Name:'var-int-blocks-example-2' }}}

`Range = 3` so bit count is `2`, causing the real range to be -1 to 2

`direction = -1` will serialize to `00`

`direction = 1` will serialize to `10`


### Generated Code

Source:
{{{ Path:'Snippets/BitPacking/VarIntBlocksSnippets.cs' Name:'var-int-blocks-generated-source' }}}

Generated:
{{{ Path:'Snippets/BitPacking/VarIntBlocksSnippets.cs' Name:'var-int-blocks-generated-code' }}}

*last updated for Mirage v101.8.0*