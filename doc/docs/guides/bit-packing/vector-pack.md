# Vector Pack

A Vector2 or Vector3 can be compressed using [Vector2PackAttribute](/docs/reference/Mirage.Serialization/Vector2PackAttribute) or [Vector3PackAttribute](/docs/reference/Mirage.Serialization/Vector3PackAttribute)

These attributes work in the same way as [FloatPack](/docs/guides/bit-packing/float-pack) except on 2 or 3 dimensions instead of 1

### Use cases

- A Vector value with a maximum value

### Supported Types

- Vector2 [Vector2PackAttribute](/docs/reference/Mirage.Serialization/Vector2PackAttribute)
- Vector3 [Vector3PackAttribute](/docs/reference/Mirage.Serialization/Vector3PackAttribute)

### Example 1

A Position in bounds +-100 in all XYZ with 0.05 precision for all axis 

```cs
public class MyNetworkBehaviour : NetworkBehaviour 
{
    [SyncVar, Vector3Pack(100f, 100f, 100f, 0.05f)]
    public Vector3 Position;
}
```

### Example 2

A Position in bounds +-100 in all XZ with 0.05 precision, but with +-20 and precision 0.1 in y-axis

```cs
public class MyNetworkBehaviour : NetworkBehaviour 
{
    [SyncVar, Vector3Pack(100f, 20f, 100f, 0.05f, 0.1f, 0.05f)]
    public Vector3 Position;
}
```

### Example 3

A position in a 2D map

```cs
public class MyNetworkBehaviour : NetworkBehaviour 
{
    [SyncVar, Vector2Pack(1000f, 80f, 0.05f)]
    public Vector2 Position;
}
```

### Generated Code

Source:
```cs 
[SyncVar, Vector3Pack(100f, 20f, 100f, 0.05f, 0.1f, 0.05f)]
public int myValue1;

[SyncVar, Vector2Pack(1000f, 80f, 0.05f)]
public int myValue2;
```

Generated:
```cs

private Vector3Packer myValue1__Packer = new Vector3Packer(1100f, 20f, 100f, 0.05f, 0.1f, 0.05f);
private Vector2Packer myValue2__Packer = new Vector2Packer(1000f, 80f, 0.05f, 0.05f);

public override bool SerializeSyncVars(NetworkWriter writer, bool initialState)
{
    ulong syncVarDirtyBits = base.SyncVarDirtyBits;
    bool result = base.SerializeSyncVars(writer, initialize);

    if (initialState) 
    {
        myValue1__Packer.Pack(writer, this.myValue1);
        myValue2__Packer.Pack(writer, this.myValue2);
        return true;
    }

    writer.Write(syncVarDirtyBits, 2);
    if ((syncVarDirtyBits & 1UL) != 0UL)
    {
        myValue1__Packer.Pack(writer, this.myValue1);
        result = true;
    }
    if ((syncVarDirtyBits & 2UL) != 0UL)
    {
        myValue2__Packer.Pack(writer, this.myValue2);
        result = true;
    }

    return result;
}

public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
{
    base.DeserializeSyncVars(reader, initialState);

    if (initialState)
    {
        this.myValue1 = myValue1__Packer.Unpack(reader);
        this.myValue2 = myValue2__Packer.Unpack(reader);
        return;
    }

    ulong dirtyMask = reader.Read(2);
    if ((dirtyMask & 1UL) != 0UL)
    {
        this.myValue1 = myValue1__Packer.Unpack(reader);
    }
    if ((dirtyMask & 2UL) != 0UL)
    {
        this.myValue2 = myValue2__Packer.Unpack(reader);
    }
}
```

*last updated for Mirage v101.8.0*