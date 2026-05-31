# MIRAGE1503: High Bit-Overhead Primitive Type

## The Problem
A primitive type (like `int`, `uint`, `long`, `ulong`, `float`, or `double`) is used in a `[SyncVar]`, RPC parameter, or `[NetworkMessage]` field without any bit-packing, compression, or range-limiting attributes.

Standard uncompressed primitives write their full bit-width (e.g. 32 bits for `int`/`float`, 64 bits for `long`/`double`) onto the network buffer, even if the runtime values are small or do not require that level of precision. Over time, this leads to unnecessary bandwidth consumption.

---

## Example of Triggering Code
```csharp
using Mirage;

public class Player : NetworkBehaviour
{
    // Warning: 'Health' uses uncompressed int which has high bit-overhead.
    [SyncVar]
    public int Health { get; set; }

    // Warning: 'PlayerScale' uses uncompressed float which has high bit-overhead.
    [SyncVar]
    public float PlayerScale { get; set; }
}
```

---

## How to Resolve

Decorate the fields/properties with appropriate compression attributes like `[BitCount]`, `[VarInt]`, `[FloatPack]`, or `[BitCountFromRange]` to minimize the serialized bit size.

```csharp
using Mirage;
using Mirage.Serialization;

public class Player : NetworkBehaviour
{
    // Correct: Restrict Health to 7 bits (0-127 range)
    [SyncVar, BitCount(7)]
    public int Health { get; set; }

    // Correct: Compress float with a defined range and precision
    [SyncVar, FloatPack(-10f, 10f, 0.01f)]
    public float PlayerScale { get; set; }
}
```
