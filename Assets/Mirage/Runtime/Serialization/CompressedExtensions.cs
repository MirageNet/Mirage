using System.Runtime.CompilerServices;
using UnityEngine;

namespace Mirage.Serialization
{
    public static class CompressedExtensions
    {
        /// <summary>
        /// Packs Quaternion using <see cref="QuaternionPacker.Default9"/>
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="rotation"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteQuaternion(this NetworkWriter writer, Quaternion rotation)
        {
            QuaternionPacker.Default9.Pack(writer, rotation);
        }

        /// <summary>
        /// Unpacks Quaternion using <see cref="QuaternionPacker.Default9"/>
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion ReadQuaternion(this NetworkReader reader)
        {
            return QuaternionPacker.Default9.Unpack(reader);
        }
    }
}
