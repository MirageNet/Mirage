using System;
using UnityEngine;

namespace Mirror
{
    public static class Compression
    {
        /// <summary>
        /// Lossy compression of normalized quaternion into 29 bits
        /// </summary>
        /// <param name="quaternion">The quaternion to compress</param>
        /// <returns>29 bits of compressed quaternion</returns>
        internal static uint Compress(Quaternion quaternion)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Decompress quaternions
        /// </summary>
        /// <param name="compressed">29 bits contained a compressed quaternion</param>
        /// <returns>The normalized quaternion</returns>
        internal static Quaternion Decompress(uint compressed)
        {
            throw new NotImplementedException();
        }
    }
}