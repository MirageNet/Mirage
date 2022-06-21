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

using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Mirage.Serialization
{
    public static class BitHelper
    {
        /// <summary>
        /// Gets the number of bits need for <paramref name="precision"/> in range negative to positive <paramref name="max"/>
        /// <para>
        /// WARNING: these methods are not fast, dont use in hotpath
        /// </para>
        /// </summary>
        /// <param name="max"></param>
        /// <param name="precision">lowest precision required, bit count will round up so real precision might be higher</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BitCount(float max, float precision)
        {
            return BitCount(max, precision, true);
        }

        /// <summary>
        /// Gets the number of bits need for <paramref name="precision"/> in range <paramref name="max"/>
        /// <para>If signed then range is negative max to positive max, If unsigned then 0 to max</para>
        /// <para>
        /// WARNING: these methods are not fast, dont use in hotpath
        /// </para>
        /// </summary>
        /// <param name="max"></param>
        /// <param name="precision">lowest precision required, bit count will round up so real precision might be higher</param>
        /// <returns></returns>
        public static int BitCount(float max, float precision, bool signed)
        {
            float multiplier = signed ? 2 : 1;
            return Mathf.FloorToInt(Mathf.Log(multiplier * max / precision, 2)) + 1;
        }

        /// <summary>
        /// Gets the number of bits need for <paramref name="max"/>
        /// <para>
        /// WARNING: these methods are not fast, dont use in hotpath
        /// </para>
        /// </summary>
        /// <param name="max"></param>
        /// <param name="precision">lowest precision required, bit count will round up so real precision might be higher</param>
        /// <returns></returns>
        public static int BitCount(ulong max)
        {
            return (int)Math.Floor(Math.Log(max, 2)) + 1;
        }
    }
}
