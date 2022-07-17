# Bit Packing

Bit packing is a form of data compression that reducing that number of bits it takes to serialize a value.

A simple example of this is an integer that is always between 0 and 100. Normally an integer will be serialized as 32 bits, but knowing its range is 100 it can be packed into only 7 bits.

## Bit Packing in Mirage

Mirage has many attributes that can be applied to SyncVars and Rpc parameters

- [BitCount](/docs/guides/bit-packing/bit-count) Sets the number of bits on an integer
- [BitCountFromRange](/docs/guides/bit-packing/bit-count-from-range) Sets the number of bits from a given range, rounding up.
- [ZigZagEncode](/docs/guides/bit-packing/zig-zag-encode) Encodes a value using [ZigZag Encoding](https://gist.github.com/mfuerstenau/ba870a29e16536fdbaba). Only useable with [BitCount](/docs/guides/bit-packing/bit-count) or [VarIntBlocks](/docs/guides/bit-packing/var-int-blocks)
- [VarInt](/docs/guides/bit-packing/var-int) Packs int to different size based on its size. Allows for 3 configurable size ranges
- [VarIntBlocks](/docs/guides/bit-packing/var-int-blocks)  Packs int to different size based on its size. Uses block size so can be used over a large range of values
- [FloatPack](/docs/guides/bit-packing/float-pack) Compresses a float value
- [VectorPack](/docs/guides/bit-packing/vector-pack) Compresses a Vector value
- [QuaternionPack](/docs/guides/bit-packing/quaternion-pack) Compresses a Quaternion value