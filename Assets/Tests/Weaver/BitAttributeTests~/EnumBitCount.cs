using System.Collections.Generic;
using Mirage;
using Mirage.Serialization;

namespace BitAttributeTests.EnumBitCount
{
    public class MyBehaviour : NetworkBehaviour
    {
        [SyncVar] public MyEnum value1;

        [SyncVar] public MyZigZagEnum value2;

        [SyncVar] public MyRangeEnum value3;

        [SyncVar] public MyULongEnum value4;

        // field attribute overrides enum attribute
        [BitCount(8)]
        [SyncVar] public MyEnum value5;

        [ClientRpc]
        public void RpcEnum(MyEnum value, MyZigZagEnum zigZag, MyRangeEnum range) { }

        [ServerRpc]
        public void RpcCollections(List<MyEnum> list, MyEnum[] array, MyEnum? nullable) { }
    }

    [NetworkMessage]
    public struct MyMessage
    {
        public MyEnum value;
        public MyRangeEnum range;
        public List<MyEnum> list;
    }

    [BitCount(2)]
    public enum MyEnum
    {
        A = 0,
        B = 1,
        C = 3,
    }

    [BitCount(4), ZigZagEncode]
    public enum MyZigZagEnum
    {
        Min = -8,
        Zero = 0,
        Max = 7,
    }

    [BitCountFromRange(-2, 2)]
    public enum MyRangeEnum
    {
        Min = -2,
        Zero = 0,
        Max = 2,
    }

    [BitCount(64)]
    public enum MyULongEnum : ulong
    {
        None = 0,
        Max = ulong.MaxValue,
    }
}
