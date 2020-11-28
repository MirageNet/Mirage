using System;
using UnityEngine;

namespace Mirror
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

        internal static uint Compress(Quaternion expected)
        {
            float absX = Mathf.Abs(expected.x),
                      absY = Mathf.Abs(expected.y),
                      absZ = Mathf.Abs(expected.z),
                      absW = Mathf.Abs(expected.w);

            ComponentType largestComponent = ComponentType.X;
            float largestValue = absX;
            if (absY > largestValue)
            {
                largestValue = absY;
                largestComponent = ComponentType.Y;
            }
            if (absZ > largestValue)
            {
                largestValue = absZ;
                largestComponent = ComponentType.Z;
            }
            if (absW > largestValue)
            {
                largestComponent = ComponentType.W;
            }

            float a, b, c;
            switch (largestComponent)
            {
                case ComponentType.X when expected.x >= 0:
                    a = expected.y;
                    b = expected.z;
                    c = expected.w;
                    break;
                case ComponentType.X when expected.x < 0:
                    a = -expected.y;
                    b = -expected.z;
                    c = -expected.w;
                    break;
                case ComponentType.Y when expected.y >= 0:
                    a = expected.x;
                    b = expected.z;
                    c = expected.w;
                    break;
                case ComponentType.Y when expected.y < 0:
                    a = -expected.x;
                    b = -expected.z;
                    c = -expected.w;
                    break;
                case ComponentType.Z when expected.z >= 0:
                    a = expected.x;
                    b = expected.y;
                    c = expected.w;
                    break;
                case ComponentType.Z when expected.z < 0:
                    a = -expected.x;
                    b = -expected.y;
                    c = -expected.w;

                    break;
                case ComponentType.W when expected.w >= 0:
                    a = expected.x;
                    b = expected.y;
                    c = expected.z;
                    break;
                case ComponentType.W when expected.w < 0:
                    a = -expected.x;
                    b = -expected.y;
                    c = -expected.z;
                    break;
                default:
                    // Should never happen!
                    throw new ArgumentOutOfRangeException("Unknown rotation component type: " +
                                                          largestComponent);
            }

            float normalizedA = (a - Minimum) / (Maximum - Minimum),
                normalizedB = (b - Minimum) / (Maximum - Minimum),
                normalizedC = (c - Minimum) / (Maximum - Minimum);

            uint integerA = (uint)Mathf.FloorToInt(normalizedA * 1024.0f + 0.5f),
                integerB = (uint)Mathf.FloorToInt(normalizedB * 1024.0f + 0.5f),
                integerC = (uint)Mathf.FloorToInt(normalizedC * 1024.0f + 0.5f);

            return (((uint)largestComponent) << 30) | (integerA << 20) | (integerB << 10) | integerC;
        }

        internal static Quaternion Decompress(uint compressed)
        {
            var largestComponentType = (ComponentType)(compressed >> 30);
            uint integerA = (compressed >> 20) & ((1 << 10) - 1),
                integerB = (compressed >> 10) & ((1 << 10) - 1),
                integerC = compressed & ((1 << 10) - 1);

            float a = integerA / 1024.0f * (Maximum - Minimum) + Minimum,
                b = integerB / 1024.0f * (Maximum - Minimum) + Minimum,
                c = integerC / 1024.0f * (Maximum - Minimum) + Minimum;

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
