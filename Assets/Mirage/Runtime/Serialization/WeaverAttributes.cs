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
    /// NOTE: bits are truncated when using this, so signed values will lose their sign.
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter)]
    public class BitCountAttribute : Attribute
    {
        /// <summary>
        /// Value should be between 1 and 64
        /// </summary>
        public int BitCount;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bitCount">Value should be between 1 and 64</param>
        public BitCountAttribute(int bitCount)
        {
            BitCount = bitCount;
        }
    }

    /// <summary>
    /// Tells weaver how many bits to sue for field
    /// <para>Only works with interager fields (byte, int, ulong, enums etc)</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter)]
    // todo update this
    public class VariableIntSizeAttribute : Attribute
    {
        /// <summary>
        /// Bit size of each block
        /// <para>how many bits per size bits,</para>
        /// <para>eg if size = 6 then values under 2^6 will be sent at 7 bits, values under 2^12 sent as 14 bits, etc</para>
        /// </summary>
        public int BlockSize { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="blockSize">Value should be between 1 and 64</param>
        public VariableIntSizeAttribute(int blockSize)
        {
            BlockSize = blockSize;
        }
    }


    /// <summary>
    /// Tells weaver how to pack a float field
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter)]
    public class PackFloatAttribute : Attribute
    {
        /// <summary>
        /// Number of bits to pack value as
        /// </summary>
        public int BitCount { get; }

        /// <summary>
        /// Smallest value
        /// <para><b>Example:</b> Resolution of 0.1 means values will be rounded to that: 1.53 will be sent as 1.5</para>
        /// <para>Values will be rounded to nearest value, so 1.58 will around up to 1.6</para>
        /// </summary>
        /// <remarks>
        /// Resolution will be used to caculate BitCount, so real resolution may be lower than resolution given by user
        /// </remarks>
        public float Resolution { get; }

        /// <summary>
        /// Max value of the float
        /// </summary>
        public float Max { get; }

        /// <summary>
        /// If Bitcount or Resolution constructor should be used
        /// </summary>
        public bool UseBitCount { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="max">Max value of the float</param>
        /// <param name="resolution">Smallest value, <see cref="Resolution"/></param>
        public PackFloatAttribute(float max, float resolution)
        {
            UseBitCount = false;
            Max = max;
            Resolution = resolution;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="max">Max value of the float</param>
        /// <param name="bitCount"></param>
        public PackFloatAttribute(float max, int bitCount)
        {
            UseBitCount = true;
            Max = max;
            BitCount = bitCount;
        }
    }
}
