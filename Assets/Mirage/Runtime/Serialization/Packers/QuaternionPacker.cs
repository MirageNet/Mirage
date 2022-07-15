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
    public sealed class QuaternionPacker
    {
        /// <summary>Default packer using 9 bits per element, 29 bits total</summary>
        public static readonly QuaternionPacker Default9 = new QuaternionPacker(9);
        /// <summary>Default packer using 10 bits per element, 32 bits total</summary>
        public static readonly QuaternionPacker Default10 = new QuaternionPacker(10);

        /// <summary>
        /// 1 / sqrt(2)
        /// </summary>
        private const float MAX_VALUE = 1f / 1.414214f;

        /// <summary>
        /// bit count per element writen
        /// </summary>
        private readonly int _bitCountPerElement;

        /// <summary>
        /// total bit count for Quaternion
        /// <para>
        /// count = 3 * perElement + 2;
        /// </para>
        /// </summary>
        private readonly int _totalBitCount;
        private readonly uint _readMask;
        private readonly FloatPacker _floatPacker;

        /// <param name="quaternionBitLength">10 per "smallest 3" is good enough for most people</param>
        public QuaternionPacker(int quaternionBitLength = 10)
        {
            // (this.BitLength - 1) because pack sign by itself
            _bitCountPerElement = quaternionBitLength;
            _totalBitCount = 2 + (quaternionBitLength * 3);
            _floatPacker = new FloatPacker(MAX_VALUE, quaternionBitLength);
            _readMask = (uint)BitMask.Mask(_bitCountPerElement);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Pack(NetworkWriter writer, Quaternion value)
        {
            QuickNormalize(ref value);

            FindLargestIndex(ref value, out var index);

            GetSmallerDimensions(index, ref value, out var a, out var b, out var c);

            // largest needs to be positive to be calculated by reader 
            // if largest is negative flip sign of others because Q = -Q
            if (value[(int)index] < 0)
            {
                a = -a;
                b = -b;
                c = -c;
            }

            // todo, should we be rounding down for abc? because if they are rounded up their sum may be greater than largest

            writer.Write(
                 ((ulong)index << (_bitCountPerElement * 3)) |
                 ((ulong)_floatPacker.PackNoClamp(a) << (_bitCountPerElement * 2)) |
                 ((ulong)_floatPacker.PackNoClamp(b) << _bitCountPerElement) |
                 _floatPacker.PackNoClamp(c),
                 _totalBitCount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void QuickNormalize(ref Quaternion quaternion)
        {
            var dot =
                (quaternion.x * quaternion.x) +
                (quaternion.y * quaternion.y) +
                (quaternion.z * quaternion.z) +
                (quaternion.w * quaternion.w);

            const float allowedEpsilon = 1E-5f;
            const float minAllowed = 1 - allowedEpsilon;
            const float maxAllowed = 1 + allowedEpsilon;
            // only normalize if dot product is outside allowed range
            if (minAllowed > dot || maxAllowed < dot)
            {
                var dotSqrt = (float)Math.Sqrt(dot);
                // rotation is 0
                if (dotSqrt < allowedEpsilon)
                {
                    // identity
                    quaternion.x = 0;
                    quaternion.y = 0;
                    quaternion.z = 0;
                    quaternion.w = 1;
                }
                else
                {
                    var iDotSqrt = 1 / dotSqrt;
                    quaternion.x *= iDotSqrt;
                    quaternion.y *= iDotSqrt;
                    quaternion.z *= iDotSqrt;
                    quaternion.w *= iDotSqrt;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void FindLargestIndex(ref Quaternion quaternion, out uint index)
        {
            var x2 = quaternion.x * quaternion.x;
            var y2 = quaternion.y * quaternion.y;
            var z2 = quaternion.z * quaternion.z;
            var w2 = quaternion.w * quaternion.w;

            index = 0;
            var current = x2;
            // check vs sq to avoid doing mathf.abs
            if (y2 > current)
            {
                index = 1;
                current = y2;
            }
            if (z2 > current)
            {
                index = 2;
                current = z2;
            }
            if (w2 > current)
            {
                index = 3;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void GetSmallerDimensions(uint largestIndex, ref Quaternion quaternion, out float a, out float b, out float c)
        {
            switch (largestIndex)
            {
                case 0:
                    a = quaternion.y;
                    b = quaternion.z;
                    c = quaternion.w;
                    return;
                case 1:
                    a = quaternion.x;
                    b = quaternion.z;
                    c = quaternion.w;
                    return;
                case 2:
                    a = quaternion.x;
                    b = quaternion.y;
                    c = quaternion.w;
                    return;
                case 3:
                    a = quaternion.x;
                    b = quaternion.y;
                    c = quaternion.z;
                    return;
                default:
                    ThrowIfOutOfRange();
                    a = b = c = default;
                    return;
            }
        }

        private static void ThrowIfOutOfRange() => throw new IndexOutOfRangeException("Invalid Quaternion index!");


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Quaternion Unpack(NetworkReader reader)
        {
            var combine = reader.Read(_totalBitCount);

            var index = (uint)(combine >> (_bitCountPerElement * 3));

            var a = _floatPacker.Unpack((uint)(combine >> (_bitCountPerElement * 2)) & _readMask);
            var b = _floatPacker.Unpack((uint)(combine >> (_bitCountPerElement * 1)) & _readMask);
            var c = _floatPacker.Unpack((uint)combine & _readMask);

            var l2 = 1 - ((a * a) + (b * b) + (c * c));
            var largest = (float)Math.Sqrt(l2);
            // this Quaternion should already be normallized because of the way that largest is calculated
            switch (index)
            {
                case 0:
                    return new Quaternion(largest, a, b, c);
                case 1:
                    return new Quaternion(a, largest, b, c);
                case 2:
                    return new Quaternion(a, b, largest, c);
                case 3:
                    return new Quaternion(a, b, c, largest);
                default:
                    ThrowIfOutOfRange();
                    return default;
            }
        }
    }
}
