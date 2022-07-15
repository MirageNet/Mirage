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
    /// <summary>
    /// Helps compresses a float into a reduced number of bits
    /// </summary>
    public sealed class FloatPacker
    {
        private readonly int _bitCount;
        private readonly float _multiplier_pack;
        private readonly float _multiplier_unpack;
        private readonly uint _mask;
        private readonly int _toNegative;

        /// <summary>max positive value, any uint value over this will be negative</summary>
        private readonly uint _midPoint;
        private readonly float _positiveMax;
        private readonly float _negativeMax;

        /// <param name="lowestPrecision">lowest precision, actual precision will be caculated from number of bits used</param>
        public FloatPacker(float max, float lowestPrecision) : this(max, lowestPrecision, true) { }

        public FloatPacker(float max, int bitCount) : this(max, bitCount, true) { }

        /// <param name="max"></param>
        /// <param name="lowestPrecision">lowest precision, actual precision will be caculated from number of bits used</param>
        /// <param name="signed">if negative values will be allowed or not</param>
        public FloatPacker(float max, float lowestPrecision, bool signed) : this(max, BitHelper.BitCount(max, lowestPrecision, signed), signed) { }

        /// <param name="signed">if negative values will be allowed or not</param>
        public FloatPacker(float max, int bitCount, bool signed)
        {
            _bitCount = bitCount;
            // not sure what max bit count should be,
            // but 30 seems reasonable since an unpacked float is already 32
            if (max == 0) throw new ArgumentException("Max can not be 0", nameof(max));
            if (bitCount < 1) throw new ArgumentException("Bit count is too low, bit count should be between 1 and 30", nameof(bitCount));
            if (bitCount > 30) throw new ArgumentException("Bit count is too high, bit count should be between 1 and 30", nameof(bitCount));

            _mask = (1u << bitCount) - 1u;

            if (signed)
            {
                _midPoint = (1u << (bitCount - 1)) - 1u;
                _toNegative = (int)(_mask + 1u);

                _positiveMax = max;
                _negativeMax = -max;
            }
            else // unsigned
            {
                _midPoint = (1u << (bitCount)) - 1u;
                _toNegative = 0;

                _positiveMax = max;
                _negativeMax = 0;
            }

            _multiplier_pack = _midPoint / max;
            _multiplier_unpack = 1 / _multiplier_pack;
        }


        /// <summary>
        /// Packs a float value into a uint
        /// <para>Clamps the value within min/max range</para>
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Pack(float value)
        {
            if (value >= _positiveMax) value = _positiveMax;
            if (value <= _negativeMax) value = _negativeMax;
            return PackNoClamp(value);
        }

        /// <summary>
        /// Packs and Writes a float value
        /// <para>Clamps the value within min/max range</para>
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Pack(NetworkWriter writer, float value)
        {
            if (value >= _positiveMax) value = _positiveMax;
            if (value <= _negativeMax) value = _negativeMax;
            PackNoClamp(writer, value);
        }

        /// <summary>
        /// Packs a float value into a uint without clamping it in range
        /// <para>
        /// WARNING: only use this method if value is always in range. Out of range values may not be unpacked correctly
        /// </para>
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint PackNoClamp(float value)
        {
            return (uint)Mathf.RoundToInt(value * _multiplier_pack) & _mask;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PackNoClamp(NetworkWriter writer, float value)
        {
            // dont need to mask value here because the Write function will mask it
            writer.Write((uint)Mathf.RoundToInt(value * _multiplier_pack), _bitCount);
        }


        /// <summary>
        /// Unpacks uint value to float
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks>
        /// Positive and Negative values need to be unpacked differnely so that they both keep same precision
        /// <para>
        ///     <b>Example 10 bits (max uint = 1023):</b>
        ///     <br />
        ///     p = precision
        ///     <para>
        ///         Positive values have uint range <c>0 to 511</c>
        ///         <br />
        ///         Unpacked: <c>0 to max</c>
        ///     </para>
        ///     <para>
        ///         Negative values have uint range <c>512 to 1023</c>
        ///         <br />
        ///         Unpacked using same as positive: <c>max+p to max*2+p</c>
        ///         <br />
        ///         but we want range <c>-max to -p</c>
        ///         <br />
        ///         so we need to subtrack <c>-1024</c> so range is <c>-512 to -1</c>
        ///         <br />
        ///         then scale to unpack to <c>-max to -p</c>
        ///     </para>
        /// </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Unpack(uint value)
        {
            if (value <= _midPoint) // positive
            {
                // 0 -> 511
                // 0 -> (max)
                return value * _multiplier_unpack;
            }
            else // negative
            {
                // max = 1024
                // 512 -> 1023
                // -max -> 0

                // doing `value - max*2` cause:
                // -512 -> -1
                return ((int)value - _toNegative) * _multiplier_unpack;
            }
        }

        /// <summary>
        /// Reads and unpacks float value
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Unpack(NetworkReader reader)
        {
            return Unpack((uint)reader.Read(_bitCount));
        }
    }
}
