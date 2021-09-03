/*
MIT License

Copyright (c) 2021 James Frowen

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System.Runtime.CompilerServices;

namespace Mirage.Serialization
{
    public static class BitMask
    {
        /// <summary>
        /// Creates mask for <paramref name="bits"/>
        /// <para>
        /// (showing 32 bits for simplify, result is 64 bit)
        /// <br/>
        /// Example bits = 4 => mask = 00000000_00000000_00000000_00001111
        /// <br/>
        /// Example bits = 10 => mask = 00000000_00000000_00000011_11111111
        /// </para>
        /// </summary>
        /// <param name="bits"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Mask(int bits)
        {
            return bits == 0 ? 0 : ulong.MaxValue >> (64 - bits);
        }

        /// <summary>
        /// Creates Mask either side of start and end
        /// <para>Note this mask is only valid for start [0..63] and end [0..64]</para>
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong OuterMask(int start, int end)
        {
            return (ulong.MaxValue << start) ^ (ulong.MaxValue >> (64 - end));
        }
    }
}
