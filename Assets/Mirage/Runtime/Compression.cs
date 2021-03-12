using System;
using UnityEngine;

namespace Mirage
{
    enum ComponentType : uint
    {
        X = 0,
        Y = 1,
        Z = 2,
        W = 3
    }

    /// <summary>
    ///     Credit to this man for converting gaffer games c code to c#
    ///     https://gist.github.com/fversnel/0497ad7ab3b81e0dc1dd
    /// </summary>
    public static class Compression
    {
        private const float Minimum = -1.0f / 1.414214f; // note: 1.0f / sqrt(2)
        private const float Maximum = +1.0f / 1.414214f;

        private const int BitsPerAxis = 10;
        private const int LargestComponentShift = BitsPerAxis * 3;
        private const int AShift = BitsPerAxis * 2;
        private const int BShift = BitsPerAxis * 1;
        private const int ComponentScale = (1 << BitsPerAxis) - 1;
        private const float ReverseScale = 1f / ComponentScale;

        internal static uint Compress(Quaternion quaternion)
        {
            float absX = Mathf.Abs(quaternion.x);
            float absY = Mathf.Abs(quaternion.y);
            float absZ = Mathf.Abs(quaternion.z);
            float absW = Mathf.Abs(quaternion.w);

            ComponentType largestComponent = ComponentType.X;
            float largestAbs = absX;
            float largest = quaternion.x;

            if (absY > largestAbs)
            {
                largestAbs = absY;
                largestComponent = ComponentType.Y;
                largest = quaternion.y;
            }
            if (absZ > largestAbs)
            {
                largestAbs = absZ;
                largestComponent = ComponentType.Z;
                largest = quaternion.z;
            }
            if (absW > largestAbs)
            {
                largestComponent = ComponentType.W;
                largest = quaternion.w;
            }

            float a = 0;
            float b = 0;
            float c = 0;
            switch (largestComponent)
            {
                case ComponentType.X:
                    a = quaternion.y;
                    b = quaternion.z;
                    c = quaternion.w;
                    break;
                case ComponentType.Y:
                    a = quaternion.x;
                    b = quaternion.z;
                    c = quaternion.w;
                    break;
                case ComponentType.Z:
                    a = quaternion.x;
                    b = quaternion.y;
                    c = quaternion.w;
                    break;
                case ComponentType.W:
                    a = quaternion.x;
                    b = quaternion.y;
                    c = quaternion.z;
                    break;
            }

            if (largest < 0)
            {
                a = -a;
                b = -b;
                c = -c;
            }

            uint integerA = ScaleToUint(a);
            uint integerB = ScaleToUint(b);
            uint integerC = ScaleToUint(c);

            return (((uint)largestComponent) << LargestComponentShift) | (integerA << AShift) | (integerB << BShift) | integerC;
        }

        private static uint ScaleToUint(float v)
        {
            float normalized = (v - Minimum) / (Maximum - Minimum);
            return (uint)Mathf.RoundToInt(normalized * ComponentScale);
        }

        private static float ScaleToFloat(uint v)
        {
            return v * ReverseScale * (Maximum - Minimum) + Minimum;
        }

        internal static Quaternion Decompress(uint compressed)
        {
            var largestComponentType = (ComponentType)(compressed >> LargestComponentShift);
            uint integerA = (compressed >> AShift) & ComponentScale;
            uint integerB = (compressed >> BShift) & ComponentScale;
            uint integerC = compressed & ComponentScale;

            float a = ScaleToFloat(integerA);
            float b = ScaleToFloat(integerB);
            float c = ScaleToFloat(integerC);

            Quaternion rotation;
            switch (largestComponentType)
            {
                case ComponentType.X:
                    // (?) y z w
                    rotation.y = a;
                    rotation.z = b;
                    rotation.w = c;
                    rotation.x = Mathf.Sqrt(1 - rotation.y * rotation.y
                                               - rotation.z * rotation.z
                                               - rotation.w * rotation.w);
                    break;
                case ComponentType.Y:
                    // x (?) z w
                    rotation.x = a;
                    rotation.z = b;
                    rotation.w = c;
                    rotation.y = Mathf.Sqrt(1 - rotation.x * rotation.x
                                               - rotation.z * rotation.z
                                               - rotation.w * rotation.w);
                    break;
                case ComponentType.Z:
                    // x y (?) w
                    rotation.x = a;
                    rotation.y = b;
                    rotation.w = c;
                    rotation.z = Mathf.Sqrt(1 - rotation.x * rotation.x
                                               - rotation.y * rotation.y
                                               - rotation.w * rotation.w);
                    break;
                case ComponentType.W:
                    // x y z (?)
                    rotation.x = a;
                    rotation.y = b;
                    rotation.z = c;
                    rotation.w = Mathf.Sqrt(1 - rotation.x * rotation.x
                                               - rotation.y * rotation.y
                                               - rotation.z * rotation.z);
                    break;
                default:
                    // Should never happen!
                    throw new ArgumentOutOfRangeException("Unknown rotation component type: " +
                                                          largestComponentType);
            }

            return rotation;
        }
    }
}
