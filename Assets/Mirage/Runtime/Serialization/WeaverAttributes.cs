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
    /// Tells weaver how many bits to sue for field
    /// <para>Only works with interager fields (byte, int, ulong, enums etc)</para>
    /// <para>
    /// NOTE: bits are truncated when using this, so signed values will lose their sign. Use <see cref="ZigZagEncodeAttribute"/> as well if value might be negative
    /// </para>
    /// <para>Also See: <see href="https://miragenet.github.io/Mirage/Articles/Guides/BitPacking/BitCount.html">Bit Packing Documentation</see></para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter)]
    public class BitCountAttribute : Attribute
    {
        /// <param name="bitCount">Value should be between 1 and 64</param>
        public BitCountAttribute(int bitCount) { }
    }

    /// <summary>
    /// Calculates bitcount from then given min/max values and then packs using <see cref="BitCountAttribute"/>
    /// <para>Also See: <see href="https://miragenet.github.io/Mirage/Articles/Guides/BitPacking/BitCountFromRange.html">Bit Packing Documentation</see></para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter)]
    public class BitCountFromRangeAttribute : Attribute
    {
        /// <param name="min">minimum possible int value</param>
        /// <param name="max">minimum possible max value</param>
        public BitCountFromRangeAttribute(int min, int max) { }
    }

    /// <summary>
    /// Used along size <see cref="BitCountAttribute"/> to encodes a interager value using <see cref="ZigZag"/> so that both positive and negative values can be sent
    /// <para>Also See: <see href="https://miragenet.github.io/Mirage/Articles/Guides/BitPacking/BitCountFromRange.html">Bit Packing Documentation</see></para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter)]
    public class ZigZagEncodeAttribute : Attribute
    {
        public ZigZagEncodeAttribute() { }
    }

    /// <summary>
    /// Packs a float field, clamped from -max to +max, with 
    /// <para>Also See: <see href="https://miragenet.github.io/Mirage/Articles/Guides/BitPacking/BitCountFromRange.html">Bit Packing Documentation</see></para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter)]
    public class FloatPackAttribute : Attribute
    {
        /// <param name="max">Max value of the float</param>
        /// <param name="precision">Smallest possible value of the field. Real precision woll be caculated using bitcount but will always be lower than this parameter</param>
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

        public Vector3PackAttribute(float xMax, float yMax, float zMax, int xBitCount, int yBitCount, int ZBitCount) { }
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
        public QuaternionPackAttribute(int bitPerElement) { }
    }
#pragma warning restore IDE0060 // Remove unused parameter
}
