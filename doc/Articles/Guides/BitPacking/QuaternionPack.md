*work in progress*
# Quaternion Pack

### Quaternion Rules

Quaternions can be packed using some of the mathematically rules they follow


#### Quaternions should be normalized

x^2 + y^2 + z^2 + w^2 = 1

Because of this we only need to send the smallest 3 value as we can calculate the largest one again on the other side.

We also need to send the index to say which of the 4 elements was the largest

largest = sqrt(1 - a^2 + b^2 + c^2)

#### Positive and Negative Quaternions represent the same rotation

Q(x,y,z,w) === Q(-x,-y,-z,-w)

If the largest element is negative we would have to send its sign in order to calculate the correct rotation.

However because Q=-Q, if the largest element is negative we can just flip the sign of all 4 elements instead.

#### Max of second largest element

The value of the 2nd largest element is when it is also equal to the largest so we have

L^2 + L^2 = 1

From this we can find the max value for 2nd largest is 

L = +- 1 / sqrt(2) = +- ~0.707

This allows us to pack the smallest 3 elements in range -0.707 to +0.707 instead of -1 to +1

#### Result

Combining all this we can send each of the smallest 3 elements with 9 bits, and 2 bits for the index of the largest element. Which reduces the size of a Quaternion from 128 bits unpacked to only 29 bits.

The precision of the smallest 3 can in increased or decreased to change the bit counts by multiples of 3. eg 10 bits per element will result is 32 bits total.


### Supported Types

- Quaternion

### Example 1

```cs
public class MyNetworkBehaviour : NetworkBehaviour 
{
    [SyncVar, QuaternionPack(9)]
    public Quaternion direction;
}
```

### Generated Code

Source:
```cs 
[SyncVar, QuaternionPack(9)]
public int myValue;
```

Generated:
```cs

private QuaternionPacker myValue__Packer = new QuaternionPacker(9);

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