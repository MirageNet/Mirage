using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Mirage.Serialization
{
    public static class CompressedExtensions
    {
        [WeaverIgnore]
        public static void PackRotation(this NetworkWriter writer, Quaternion rotation)
        {
            QuaternionPacker.Default.Pack(writer, rotation);
        }
        [WeaverIgnore]
        public static unsafe void PackFloatSigned(this NetworkWriter writer, float value, float signedMaxFloat, int bitCount)
        {
            // todo should we scale value by maxFloat first, then cast to int?

            int intValue = *(int*)&value;
            int mask = intValue >> 31;
            int absValue = (intValue + mask) ^ mask;
            // 0=> positive, 1 => negative
            int sign = mask;

            writer.Write((uint)sign, 1);

            // scale value to 0->1 (as float)
            //   doesn't matter if value is greater than max, it will just be over 
            float valueRelative = value / signedMaxFloat;

            // scale to fit into bits
            uint maxUint = uint.MaxValue >> (32 - bitCount);
            float outValue = (valueRelative * maxUint) + 0.5f;
            writer.Write((uint)outValue, bitCount - 1);
        }

        [WeaverIgnore]
        public static Quaternion UnPackRotation(this NetworkReader reader)
        {
            return QuaternionPacker.Default.Unpack(reader);
        }
        [WeaverIgnore]
        public static unsafe float UnpackFloatSigned(this NetworkReader reader, float signedMaxFloat, int bitCount)
        {
            ulong sign = reader.ReadBooleanAsUlong();
            uint compressed = (uint)reader.Read(bitCount - 1);

            // todo do we need to subtract 0.5f here???
            float floatValue = compressed;

            uint maxUint = uint.MaxValue >> (32 - bitCount);
            float outValue = (floatValue / maxUint) * signedMaxFloat;

            (*(int*)&outValue) |= ((int)sign << 31);

            // todo alt sign (benchmark this vs above)
            // 0=> positive, 1 => negative
            // shift 1,==> 0=> positive, 2 => negative
            // 1-s, ==> 1=> positive, -1 => negative
            // int intSign = 1 - (((int)sign) << 1);

            return outValue;
        }
    }
    public class QuaternionPacker
    {
        public static readonly QuaternionPacker Default = new QuaternionPacker();

        /// <summary>
        /// 1 / sqrt(2)
        /// </summary>
        const float MaxValue = 1f / 1.414214f;

        readonly int BitLength;

        /// <summary>
        /// Mathf.Pow(1, targetBitLength) - 1
        /// <para>
        /// Can also be used as mask
        /// </para>
        /// </summary>
        readonly uint UintMax;

        /// <summary>
        /// bit count per element writen
        /// </summary>
        public readonly int bitCountPerElement;

        /// <summary>
        /// total bit count for Quaternion
        /// <para>
        /// count = 3 * perElement + 2;
        /// </para>
        /// </summary>
        public readonly int bitCount;

        /// <param name="quaternionBitLength">10 per "smallest 3" is good enough for most people</param>
        public QuaternionPacker(int quaternionBitLength = 10)
        {
            BitLength = quaternionBitLength;
            // (this.BitLength - 1) because pack sign by itself
            UintMax = (1u << (BitLength - 1)) - 1u;
            bitCountPerElement = quaternionBitLength;
            bitCount = 2 + (quaternionBitLength * 3);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Pack(NetworkWriter writer, Quaternion _value)
        {
            // make sure value is normalized (dont trust user given value, and math here assumes normalized)
            float x = _value.x;
            float y = _value.y;
            float z = _value.z;
            float w = _value.w;

            quickNormalize(ref x, ref y, ref z, ref w);

            FindLargestIndex(x, y, z, w, out int index, out float largest);

            GetSmallerDimensions(index, x, y, z, w, out float a, out float b, out float c);

            // largest needs to be positive to be calculated by reader 
            // if largest is negative flip sign of others because Q = -Q
            if (largest < 0)
            {
                a = -a;
                b = -b;
                c = -c;
            }

            writer.Write((uint)index, 2);
            writer.PackFloatSigned(a, MaxValue, BitLength);
            writer.PackFloatSigned(b, MaxValue, BitLength);
            writer.PackFloatSigned(c, MaxValue, BitLength);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void quickNormalize(ref float x, ref float y, ref float z, ref float w)
        {
            float dot =
                (x * x) +
                (y * y) +
                (z * z) +
                (w * w);
            const float allowedEpsilon = 1E-5f;
            const float minAllowed = 1 - allowedEpsilon;
            const float maxAllowed = 1 + allowedEpsilon;
            if (minAllowed > dot || maxAllowed < dot)
            {
                float dotSqrt = (float)Math.Sqrt(dot);
                // rotation is 0
                if (dotSqrt < allowedEpsilon)
                {
                    // identity
                    x = 0;
                    y = 0;
                    z = 0;
                    w = 1;
                }
                else
                {
                    x /= dotSqrt;
                    y /= dotSqrt;
                    z /= dotSqrt;
                    w /= dotSqrt;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void FindLargestIndex(float x, float y, float z, float w, out int index, out float largest)
        {
            float x2 = x * x;
            float y2 = y * y;
            float z2 = z * z;
            float w2 = w * w;

            index = 0;
            float current = x2;
            largest = x;
            // check vs sq to avoid doing mathf.abs
            if (y2 > current)
            {
                index = 1;
                largest = y;
                current = y2;
            }
            if (z2 > current)
            {
                index = 2;
                largest = z;
                current = z2;
            }
            if (w2 > current)
            {
                index = 3;
                largest = w;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void GetSmallerDimensions(int largestIndex, float x, float y, float z, float w, out float a, out float b, out float c)
        {
            switch (largestIndex)
            {
                case 0:
                    a = y;
                    b = z;
                    c = w;
                    return;
                case 1:
                    a = x;
                    b = z;
                    c = w;
                    return;
                case 2:
                    a = x;
                    b = y;
                    c = w;
                    return;
                case 3:
                    a = x;
                    b = y;
                    c = z;
                    return;
                default:
                    throw new IndexOutOfRangeException("Invalid Quaternion index!");
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Quaternion Unpack(NetworkReader reader)
        {
            Quaternion result;

            uint index = (uint)reader.Read(2);

            float a = reader.UnpackFloatSigned(MaxValue, BitLength);
            float b = reader.UnpackFloatSigned(MaxValue, BitLength);
            float c = reader.UnpackFloatSigned(MaxValue, BitLength);

            result = FromSmallerDimensions(index, a, b, c);

            return result;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static Quaternion FromSmallerDimensions(uint largestIndex, float a, float b, float c)
        {
            float l2 = 1 - ((a * a) + (b * b) + (c * c));
            float largest = (float)Math.Sqrt(l2);
            // this Quaternion should already be normallized because of the way that largest is calculated
            // todo create test to validate that result is normalized
            switch (largestIndex)
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
                    throw new IndexOutOfRangeException("Invalid Quaternion index!");

            }
        }
    }
}
