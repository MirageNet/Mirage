using System.Runtime.CompilerServices;

namespace Mirage.Serialization
{
    /// <summary>
    /// See <see href="https://gist.github.com/mfuerstenau/ba870a29e16536fdbaba">zigzag encoding</see><br/>
    /// </summary>
    public static class ZigZag
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Encode(int v)
        {
            return (uint)((v >> 31) ^ (v << 1));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Encode(long v)
        {
            return (ulong)((v >> 63) ^ (v << 1));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Decode(uint v)
        {
            return (int)((v >> 1) ^ -(v & 1));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Decode(ulong v)
        {
            return ((long)(v >> 1)) ^ -((long)v & 1);
        }
    }
}
