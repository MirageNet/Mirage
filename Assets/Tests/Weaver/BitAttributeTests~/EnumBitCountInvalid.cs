using Mirage;
using Mirage.Serialization;

namespace BitAttributeTests.EnumBitCountInvalid
{
    public class MyBehaviour : NetworkBehaviour
    {
        // error, field bit count is too small for enum values
        [BitCount(1)]
        [SyncVar] public FieldTooSmall value1;

        // using invalid enum should not log the enum's error again
        [SyncVar] public TooLarge value2;
    }

    public enum FieldTooSmall
    {
        A = 0,
        B = 2,
    }

    // error, value doesn't fit
    [BitCount(2)]
    public enum TooLarge
    {
        A = 0,
        B = 4,
    }

    // error, negative without ZigZag
    [BitCount(4)]
    public enum Negative
    {
        A = -1,
        B = 0,
    }

    // error, zigzag encoded value doesn't fit
    [BitCount(4), ZigZagEncode]
    public enum ZigZagTooLarge
    {
        A = 0,
        B = 8,
    }

    // error, value is below range
    [BitCountFromRange(-1, 3)]
    public enum BelowRange
    {
        A = -2,
        B = 0,
    }

    // error, bit count above type size
    [BitCount(9)]
    public enum TooManyBits : byte
    {
        A = 0,
    }

    // error, ZigZag can't be used by itself
    [ZigZagEncode]
    public enum ZigZagOnly
    {
        A = 0,
    }
}
