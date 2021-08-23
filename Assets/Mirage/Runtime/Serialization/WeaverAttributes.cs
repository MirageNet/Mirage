using System;

namespace Mirage.Serialization
{
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
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter)]
    public class BitCountAttribute : Attribute
    {
        /// <summary>
        /// Value should be between 1 and 64
        /// </summary>
        public int BitCount { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bitCount">Value should be between 1 and 64</param>
        public BitCountAttribute(int bitCount)
        {
            BitCount = bitCount;
        }
    }
    
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter)]
    public class BitCountFromRangeAttribute : Attribute
    {
        public BitCountFromRangeAttribute(int min, int max) { }
    }

    /// <summary>
    /// Used along size <see cref="BitCountAttribute"/> to encodes a interager value using <see cref="ZigZag"/> so that both positive and negative values can be sent
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter)]
    public class ZigZagEncodeAttribute : Attribute
    {
        public ZigZagEncodeAttribute() { }
    }
}
