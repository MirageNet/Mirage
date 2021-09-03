# Bit Count

The bit count of Integer based fields can be set using <xref:Mirage.Serialization.BitCountAttribute> 

This will truncate the bits so that only the small bits are sent. There is no range checking for values using BitCount, so value that are too big or negative will not be unpacked correctly

This means that `BitCount` should not be used with values that can be negative because this data will be lost. If you do need to send negative values then use [ZigZagEncode](./ZigZagEncode.md) or [BitCountFromRange](./BitCountFromRange.md)

### Use cases

- A Value with a maximum value
- An index in an array of known size
    - eg array with 10 elements, index can be sent as 4 bits
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

```cs
public class MyNetworkBehaviour : NetworkBehaviour 
{
    [SyncVar, BitCount(7)]
    public int Health;
}
```

`BitCount = 7` so max value of Health is `127`

`health = 57` will serialize to `011_1001`

`health = -1` *(out of range)* will serialize to `111_1111`

`health = 130` *(out of range)* will serialize to `000_0010`


### Example 2

Weapon index in a list of 6 weapons
```cs
public class MyNetworkBehaviour : NetworkBehaviour 
{
    [SyncVar, BitCount(3)]
    public int WeaponIndex;
}
```

`BitCount = 7` so max value of Health is 7

`WeaponIndex = 5` will serialize to `101`


### Generated Code

Source:
```cs 
[SyncVar, BitCount(7)]
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
        writer.Write((ulong)this.myValue, 7);
        return true;
    }

    writer.Write(syncVarDirtyBits, 1);
    if ((syncVarDirtyBits & 1UL) != 0UL)
    {
        writer.Write((ulong)this.myValue, 7);
        result = true;
    }

    return result;
}

public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
{
    base.DeserializeSyncVars(reader, initialState);

    if (initialState)
    {
        this.myValue = reader.Read(7);
        return;
    }

    ulong dirtyMask = reader.Read(1);
    if ((dirtyMask & 1UL) != 0UL)
    {
        this.myValue = reader.Read(7);
    }
}
```

*last updated for Mirage v101.8.0*