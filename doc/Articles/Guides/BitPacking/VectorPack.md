# Vector Pack

A Vector2 or Vector3 can be compressed using <xref:Mirage.Serialization.Vector2PackAttribute> or <xref:Mirage.Serialization.Vector3PackAttribute> 

This attributes work in the same way as [FloatPack](./FloatPack.md) expect on 2 or 3 dimensions instead of 1

### Use cases

- A Vector value with a maximum value

### Supported Types

- Vector2 (<xref:Mirage.Serialization.Vector2PackAttribute>)
- Vector3 (<xref:Mirage.Serialization.Vector3PackAttribute>)

### Example 1

Health which is between 0 and 100

```cs
public class MyNetworkBehaviour : NetworkBehaviour 
{
    [SyncVar, FloatPack(100f, 0.02f)]
    public int Health;
}
```

`Max = 100`, `resolution = 0.02f` so bit count is 14

`health = 57.2f` will serialize to `01_0010_0100_1101` and deserialize to `57.197f`

`health = -13.5f` will serialize to `11_1011_1010_1110` and deserialize to `-13.503f`

`health = 120f` will be clamped to `100f`


### Example 2

A Percent that where you only want to send 8 bits

```cs
public class MyNetworkBehaviour : NetworkBehaviour 
{
    [SyncVar, FloatPack(1f, 8)]
    public int Percent;
}
```

`Max = 1f`, `bitCount = 8` so resolution will be `0.00787f`

### Generated Code

Source:
```cs 
[SyncVar, FloatPack(100f, 0.02f)]
public int myValue;
```

Generated:
```cs

private FloatPacker myValue__Packer = new FloatPacker(100f, 0.02f);

public override bool SerializeSyncVars(NetworkWriter writer, bool initialState)
{
    ulong syncVarDirtyBits = base.SyncVarDirtyBits;
    bool result = base.SerializeSyncVars(writer, initialize);

    if (initialState) 
    {
        myValue__Packer.Pack(writer, this.myValue);
        return true;
    }

    writer.Write(syncVarDirtyBits, 1);
    if ((syncVarDirtyBits & 1UL) != 0UL)
    {
        myValue__Packer.Pack(writer, this.myValue);
        result = true;
    }

    return result;
}

public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
{
    base.DeserializeSyncVars(reader, initialState);

    if (initialState)
    {
        this.myValue = myValue__Packer.Unpack(reader);
        return;
    }

    ulong dirtyMask = reader.Read(1);
    if ((dirtyMask & 1UL) != 0UL)
    {
        this.myValue = myValue__Packer.Unpack(reader);
    }
}
```

*last updated for Mirage v101.8.0*