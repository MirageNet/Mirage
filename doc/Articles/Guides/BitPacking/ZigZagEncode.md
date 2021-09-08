# ZigZag Encode

To encoding a value using [ZigZag Encoding](https://gist.github.com/mfuerstenau/ba870a29e16536fdbaba) you can use the <xref:Mirage.Serialization.ZigZagEncodeAttribute> 

This will cause negative values to be encoded as positive so that the sign bit is not lost when packing.

This works best with [VarIntBlocks](./VarIntBlocks.md) but also works with [BitCount](./BitCount.md).

This attribute can not be used on the same field as [BitCountFromRange](./BitCountFromRange.md), this is because `BitCountFromRange` already ensures negative values are packed correctly.

> [!NOTE]
> The sign of a value will take up 1 bit, so if the value is in range -+100 it will need a bit count of 8

### Use cases

- A value that can be negative or positive

### Supported Types

- Byte
- Short
- Int
- Long
- Enum

### Example 1

A modifier which can add to a character value to increase or decrease it

```cs
public class MyNetworkBehaviour : NetworkBehaviour 
{
    [SyncVar, BitCount(8), ZigZagEncode]
    public int modifier;
}
```

`Range = 200` so bit count is 8, causing real range to be -128 to 127

`modifier = 57` will serialize to `0111_0010`

`modifier = -57` will serialize to `0111_0001`

`modifier = -110` will serialize to `1101_1011`, even tho -110 is out of range there is enough range because bit count rounds up.

`modifier = 130` *(out of range)* will serialize to `0000_0100`

### Generated Code

Source:
```cs 
[SyncVar, BitCount(8), ZigZagEncode]
public int myValue;
```

Generated:
```cs
public override bool SerializeSyncVars(NetworkWriter writer, bool initialState)
{
    ulong syncVarDirtyBits = base.SyncVarDirtyBits;
    bool result = base.SerializeSyncVars(writer, initialize);

    if (initialState) 
    {
        writer.Write((ulong)ZigZag.Encode(this.myValue), 8);
        return true;
    }

    writer.Write(syncVarDirtyBits, 1);
    if ((syncVarDirtyBits & 1UL) != 0UL)
    {
        writer.Write((ulong)ZigZag.Encode(this.myValue), 8);
        result = true;
    }

    return result;
}

public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
{
    base.DeserializeSyncVars(reader, initialState);

    if (initialState)
    {
        this.myValue = ZigZag.Decode(reader.Read(8));
        return;
    }

    ulong dirtyMask = reader.Read(1);
    if ((dirtyMask & 1UL) != 0UL)
    {
        this.myValue = ZigZag.Decode(reader.Read(8));
    }
}
```

*last updated for Mirage v101.8.0*