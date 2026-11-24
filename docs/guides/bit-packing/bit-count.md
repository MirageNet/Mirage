# Bit Count

The bit count of Integer based fields can be set using the [BitCountAttribute](/docs/reference/Mirage.Serialization/BitCountAttribute).

This will truncate the bits so that only the small bits are sent. There is no runtime range checking for values using BitCount, so the value that is too big or negative will not be unpacked correctly. Enum values are checked when weaving, see [Enum Types](#enum-types).

This means that `BitCount` should not be used with values that can be negative because this data will be lost. If you do need to send negative values then use [ZigZagEncode](/docs/guides/bit-packing/zig-zag-encode) or [BitCountFromRange](/docs/guides/bit-packing/bit-count-from-range)

### Use cases

- A Value with a maximum value
- An index in an array of known size
    - eg array with 10 elements, the index can be sent as 4 bits
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

`BitCount = 3` so max value of Health is 7

`WeaponIndex = 5` will serialize to `101`


### Enum Types

`BitCount` can also be placed on an enum type. The bit count is then used everywhere that enum is sent:
- SyncVars
- RPC parameters
- Fields in messages and structs
- Collections and nullables, eg `List<T>`, `T[]` and `T?`

A `BitCount` on a field or parameter will override the one on the enum.

```cs
[BitCount(2)]
 public enum WeaponType
 {
     Pistol = 0,
     Rifle = 1,
     Shotgun = 2,
     Rocket = 3,
 }

 public class MyNetworkBehaviour : NetworkBehaviour
 {
     // sent using 2 bits, from the attribute on WeaponType
     [SyncVar]
     public WeaponType PrimaryWeapon;

     // attribute on field overrides the one on WeaponType
     [SyncVar, BitCount(4)]
     public WeaponType SecondaryWeapon;

     // each element in the list is sent using 2 bits
     [ClientRpc]
     public void RpcSetLoadout(System.Collections.Generic.List<WeaponType> weapons) { }
 }
```

[ZigZagEncode](/docs/guides/bit-packing/zig-zag-encode) and [BitCountFromRange](/docs/guides/bit-packing/bit-count-from-range) can also be placed on an enum type.

#### Weave time checks

When an enum is packed using `BitCount`, either from the enum type or from a field, Weaver checks every value defined in the enum. Weaver will give an error if:
- A value does not fit in the bit count
- A value is negative and `ZigZagEncode` is not used

This means that adding a new value to an enum that no longer fits will fail to compile, instead of being silently truncated.

:::note
Only defined values are checked. A value created by casting, eg `(WeaponType)10`, will still be truncated.

If you write your own write function for the enum, it will be used instead and the attribute on the enum is ignored.
:::


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
     bool result = base.SerializeSyncVars(writer, initialState);

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
         this.myValue = (int)reader.Read(7);
         return;
     }

     ulong dirtyMask = reader.Read(1);
     if ((dirtyMask & 1UL) != 0UL)
     {
         this.myValue = (int)reader.Read(7);
     }
 }
```

*Last updated for Mirage v101.8.0.*