# MIRAGE1503: High Bit-Overhead Primitive Type

## The Problem
A primitive type (like `int`, `uint`, `long`, `ulong`, `float`, or `double`) is used in a `[SyncVar]`, RPC parameter, or `[NetworkMessage]` field without any bit-packing, compression, or range-limiting attributes.

Standard uncompressed primitives write their full bit-width (e.g. 32 bits for `int`/`float`, 64 bits for `long`/`double`) onto the network buffer, even if the runtime values are small or do not require that level of precision. Over time, this leads to unnecessary bandwidth consumption.

---

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1503.cs' Name:'mirage1503-triggering' }}}

---

## How to Resolve

Decorate the fields/properties with appropriate compression attributes like `[BitCount]`, `[VarInt]`, `[FloatPack]`, or `[BitCountFromRange]` to minimize the serialized bit size.

{{{ Path:'Snippets/Analyzers/Mirage1503.cs' Name:'mirage1503-resolved' }}}
