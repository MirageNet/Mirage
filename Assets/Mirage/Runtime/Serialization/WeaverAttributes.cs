using System;

namespace Mirage.Serialization
{
    // weaver doesn't need constructor parameters to be used, so we can have constructor without fields/properties
#pragma warning disable IDE0060 // Remove unused parameter

    /// <summary>
    /// Tells Weaver to ignore an Extension method
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class WeaverIgnoreAttribute : Attribute { }

    /// <summary>
    /// Tells Weaver to serialize a type as generic instead of creating a custom functions.
    /// <para>
    /// Use this when you have created and assigned your own Read/Write functions 
    /// or when you can't use extension methods for types and need to manually assign them.
    /// </para>
    /// <para>
    /// This will cause Weaver to use the <see cref="Writer{T}.Write"/> and <see cref="Reader{T}.Read"/> generic functions instead of creating new ones.
    /// You must set these functions manually when using this attribute.
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Interface)]
    public sealed class WeaverWriteAsGenericAttribute : Attribute { }

    /// <summary>
    /// Tells weaver how many bits to sue for field
    /// <para>Only works with integer fields (byte, int, ulong, enums etc)</para>
    /// <para>
    /// NOTE: bits are truncated when using this, so signed values will lose their sign. Use <see cref="ZigZagEncodeAttribute"/> as well if value might be negative
    /// </para>
    /// <para>Also See: <see href="https://miragenet.github.io/Mirage/docs/guides/bit-packing/bit-countl">Bit Packing Documentation</see></para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter)]
    public class BitCountAttribute : Attribute
    {
        /// <param name="bitCount">Value should be between 1 and 64</param>
        public BitCountAttribute(int bitCount) { }
    }

    /// <summary>
    /// Calculates bitcount from then given min/max values and then packs using <see cref="BitCountAttribute"/>
    /// <para>Also See: <see href="https://miragenet.github.io/Mirage/docs/guides/bit-packing/bit-count-from-range">Bit Packing Documentation</see></para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter)]
    public class BitCountFromRangeAttribute : Attribute
    {
        /// <param name="min">minimum possible int value</param>
        /// <param name="max">minimum possible max value</param>
        public BitCountFromRangeAttribute(int min, int max) { }
    }

    /// <summary>
    /// Used along size <see cref="BitCountAttribute"/> to encodes a integer value using <see cref="ZigZag"/> so that both positive and negative values can be sent
    /// <para>Also See: <see href="https://miragenet.github.io/Mirage/docs/guides/bit-packing/bit-count-from-rangel">Bit Packing Documentation</see></para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter)]
    public class ZigZagEncodeAttribute : Attribute
    {
        public ZigZagEncodeAttribute() { }
    }

    /// <summary>
    /// Packs a float field, clamped from -max to +max, with
    /// <para>Also See: <see href="https://miragenet.github.io/Mirage/docs/guides/bit-packing/bit-count-from-range">Bit Packing Documentation</see></para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter)]
    public class FloatPackAttribute : Attribute
    {
        /// <param name="max">Max value of the float</param>
        /// <param name="precision">Smallest possible value of the field. Real precision will be calculated using bitcount but will always be lower than this parameter</param>
        public FloatPackAttribute(float max, float precision) { }

        /// <param name="max">Max value of the float</param>
        /// <param name="bitCount">number of bits to pack the field into</param>
        public FloatPackAttribute(float max, int bitCount) { }
    }

    /// <summary>
    ///
    /// </summary>
    public class Vector3PackAttribute : Attribute
    {
        public Vector3PackAttribute(float xMax, float yMax, float zMax, float xPrecision, float yPrecision, float zPrecision) { }
        public Vector3PackAttribute(float xMax, float yMax, float zMax, float precision) { }

        public Vector3PackAttribute(float xMax, float yMax, float zMax, int xBitCount, int yBitCount, int zBitCount) { }
        public Vector3PackAttribute(float xMax, float yMax, float zMax, int bitCount) { }
    }

    /// <summary>
    ///
    /// </summary>
    public class Vector2PackAttribute : Attribute
    {
        public Vector2PackAttribute(float xMax, float yMax, float xPrecision, float yPrecision) { }
        public Vector2PackAttribute(float xMax, float yMax, float precision) { }

        public Vector2PackAttribute(float xMax, float yMax, int xBitCount, int yBitCount) { }
        public Vector2PackAttribute(float xMax, float yMax, int bitCount) { }
    }

    /// <summary>
    ///
    /// </summary>
    public class QuaternionPackAttribute : Attribute
    {
        public QuaternionPackAttribute(int bitPerElement = 9) { }
    }

    /// <summary>
    /// Tells weaver the max range for small, medium and large values.
    /// <para>Allows small values to be sent using less bits</para>
    /// <para>Only works with integer fields (byte, int, ulong, enums etc)</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter)]
    public class VarIntAttribute : Attribute
    {
        public VarIntAttribute(ulong smallMax, ulong mediumMax) { }
        public VarIntAttribute(ulong smallMax, ulong mediumMax, ulong largeMax, bool throwIfOverLarge = true) { }
    }



    /// <summary>
    /// Tells weaver the block size to use for packing int values
    /// <para>Allows small values to be sent using less bits</para>
    /// <para>Only works with integer fields (byte, int, ulong, enums etc)</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter)]
    public class VarIntBlocksAttribute : Attribute
    {
        /// <summary>
        /// Bit size of each block
        /// <para>how many bits per size bits,</para>
        /// <para>eg if size = 6 then values under 2^6 will be sent at 7 bits, values under 2^12 sent as 14 bits, etc</para>
        /// </summary>

        /// <summary>
        ///
        /// </summary>
        /// <param name="blockSize">Value should be between 1 and 64</param>
        public VarIntBlocksAttribute(int blockSize) { }
    }
#pragma warning restore IDE0060 // Remove unused parameter

    /// <summary>
    /// The max value for N number of bits
    /// </summary>
    /// <example>
    /// Using FromBitCount with <see cref="VarIntAttribute"/> because it uses max value not bit count
    /// <code>
    /// [VarInt(FromBitCount.b3, FromBitCount.b7, FromBitCount.b10, true)]
    /// public int componentIndex;
    /// </code>
    /// </example>
    public static class FromBitCount
    {
#pragma warning disable IDE1006 // Naming Styles
        public const ulong b1 = (1ul << 1) - 1;
        public const ulong b2 = (1ul << 2) - 1;
        public const ulong b3 = (1ul << 3) - 1;
        public const ulong b4 = (1ul << 4) - 1;
        public const ulong b5 = (1ul << 5) - 1;
        public const ulong b6 = (1ul << 6) - 1;
        public const ulong b7 = (1ul << 7) - 1;
        public const ulong b8 = (1ul << 8) - 1;
        public const ulong b9 = (1ul << 9) - 1;
        public const ulong b10 = (1ul << 10) - 1;
        public const ulong b11 = (1ul << 11) - 1;
        public const ulong b12 = (1ul << 12) - 1;
        public const ulong b13 = (1ul << 13) - 1;
        public const ulong b14 = (1ul << 14) - 1;
        public const ulong b15 = (1ul << 15) - 1;
        public const ulong b16 = (1ul << 16) - 1;
        public const ulong b17 = (1ul << 17) - 1;
        public const ulong b18 = (1ul << 18) - 1;
        public const ulong b19 = (1ul << 19) - 1;
        public const ulong b20 = (1ul << 20) - 1;
        public const ulong b21 = (1ul << 21) - 1;
        public const ulong b22 = (1ul << 22) - 1;
        public const ulong b23 = (1ul << 23) - 1;
        public const ulong b24 = (1ul << 24) - 1;
        public const ulong b25 = (1ul << 25) - 1;
        public const ulong b26 = (1ul << 26) - 1;
        public const ulong b27 = (1ul << 27) - 1;
        public const ulong b28 = (1ul << 28) - 1;
        public const ulong b29 = (1ul << 29) - 1;
        public const ulong b30 = (1ul << 30) - 1;
        public const ulong b31 = (1ul << 31) - 1;
        public const ulong b32 = (1ul << 32) - 1;
        public const ulong b33 = (1ul << 33) - 1;
        public const ulong b34 = (1ul << 34) - 1;
        public const ulong b35 = (1ul << 35) - 1;
        public const ulong b36 = (1ul << 36) - 1;
        public const ulong b37 = (1ul << 37) - 1;
        public const ulong b38 = (1ul << 38) - 1;
        public const ulong b39 = (1ul << 39) - 1;
        public const ulong b40 = (1ul << 40) - 1;
        public const ulong b41 = (1ul << 41) - 1;
        public const ulong b42 = (1ul << 42) - 1;
        public const ulong b43 = (1ul << 43) - 1;
        public const ulong b44 = (1ul << 44) - 1;
        public const ulong b45 = (1ul << 45) - 1;
        public const ulong b46 = (1ul << 46) - 1;
        public const ulong b47 = (1ul << 47) - 1;
        public const ulong b48 = (1ul << 48) - 1;
        public const ulong b49 = (1ul << 49) - 1;
        public const ulong b50 = (1ul << 50) - 1;
        public const ulong b51 = (1ul << 51) - 1;
        public const ulong b52 = (1ul << 52) - 1;
        public const ulong b53 = (1ul << 53) - 1;
        public const ulong b54 = (1ul << 54) - 1;
        public const ulong b55 = (1ul << 55) - 1;
        public const ulong b56 = (1ul << 56) - 1;
        public const ulong b57 = (1ul << 57) - 1;
        public const ulong b58 = (1ul << 58) - 1;
        public const ulong b59 = (1ul << 59) - 1;
        public const ulong b60 = (1ul << 60) - 1;
        public const ulong b61 = (1ul << 61) - 1;
        public const ulong b62 = (1ul << 62) - 1;
        public const ulong b63 = (1ul << 63) - 1;
        public const ulong b64 = ulong.MaxValue;
#pragma warning restore IDE1006 // Naming Styles
    }
}
