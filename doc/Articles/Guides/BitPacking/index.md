# Bit Packing

Bit packing is a form of data compression that reducing that number of bits it takes to serialize a value.

A simple example of this is an integer that is always between 0 and 100. Normally an integer will be serialized as 32 bits, but knowing its range is 100 it can be packed into only 7 bits.

## Bit Packing in Mirage

Mirage has many attributes that can be applied to SyncVars and Rpc parameters

- [BitCount](./BitCount.md) Sets the number of bits on an integer
- [BitCountFromRange](./BitCountFromRange.md) Sets the number of bits from a given range, rounding up.
- [ZigZagEncode](./ZigZagEncode.md) Encodes a value using [ZigZag Encoding](https://gist.github.com/mfuerstenau/ba870a29e16536fdbaba). Only useable with [BitCount](./BitCount.md) or [VarIntBlocks](./VarIntBlocks.md)
- [VarInt](./VarInt.md) Packs int to different size based on its size. Allows for 3 configurable size ranges
- [VarIntBlocks](./VarIntBlocks.md)  Packs int to different size based on its size. Uses block size so can be used over a large range of values
- [FloatPack](./FloatPack.md) Compresses a float value
- [VectorPack](./VectorPack.md) Compresses a Vector value
- [QuaternionPack](./QuaternionPack.md) Compresses a Quaternion value