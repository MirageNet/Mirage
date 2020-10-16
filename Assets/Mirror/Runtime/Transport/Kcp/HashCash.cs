
using System;
using UnityEngine;

namespace Mirror.KCP
{

    /// <summary>
    /// minimalistic hashcash-like token
    /// a simplification of the hashcash header described here:
    /// https://github.com/cliftonm/hashcash
    /// </summary>
    /// <remarks> to make it light weight in the server I adjusted the field to plain numbers</remarks>
    /// <remarks> When hashing this structure with sha1
    /// for a token to validate it has to:
    /// 1) be recent,  so dt must be in the near past in utc time
    /// 2) the resource must match the expected resource in the server
    /// 3) the token has not been seen in the server yet
    /// 4) the sha1 hash of the token must start with zeroes. The more zeroes,  the more difficulty
    public struct HashCash : IEquatable<HashCash>
    {
        /// <summary>
        /// Date and time when the token was generated
        /// </summary>
        public DateTime dt;
        /// <summary>
        /// a number that represents the resource this hashcash is for.
        /// In the original they have a string,  so just use a string hash here
        /// </summary>
        public int resource;

        /// <summary>
        /// the random number.  In the original they have
        /// a byte[],  but we can limit it to 64 bits
        /// </summary>
        public ulong salt;
        /// <summary>
        /// counter used for making the token valid
        /// </summary>
        public ulong counter;

        public bool Equals(HashCash other)
        {
            return dt == other.dt &&
                resource == other.resource &&
                salt == other.salt &&
                counter == other.counter;
        }

        public override bool Equals(object obj)
        {
            return this.Equals((HashCash)obj);
        }

        public override int GetHashCode()
        {
            int hashCode = -858276690;
            hashCode = hashCode * -1521134295 + dt.GetHashCode();
            hashCode = hashCode * -1521134295 + resource.GetHashCode();
            hashCode = hashCode * -1521134295 + salt.GetHashCode();
            hashCode = hashCode * -1521134295 + counter.GetHashCode();
            return hashCode;
        }
    }

    public static class HashCashEncoding
    {
        /// <summary>
        /// Encode a hashcash token into a buffer
        /// </summary>
        /// <param name="buffer">the buffer where to store the hashcash</param>
        /// <param name="index">the index in the buffer where to put it</param>
        /// <param name="hashCash">the token to be encoded</param>
        /// <returns>the length of the written data</returns>
        public static int Encode(byte[] buffer, int index, HashCash hashCash)
        {
            int offset = index;

            offset += Utils.Encode64U(buffer, offset, (ulong)hashCash.dt.Ticks);
            offset += Utils.Encode32U(buffer, offset, (uint)hashCash.resource);
            offset += Utils.Encode64U(buffer, offset, hashCash.salt);
            offset += Utils.Encode64U(buffer, offset, hashCash.counter);

            return offset - index ;
        }

        /// <summary>
        /// Encode a hashcash token into a buffer
        /// </summary>
        /// <param name="buffer">the buffer where to store the hashcash</param>
        /// <param name="index">the index in the buffer where to put it</param>
        /// <param name="hashCash">the token to be encoded</param>
        /// <returns>the length of the written data</returns>
        public static (int offset, HashCash decoded) Decode(byte[] buffer, int index)
        {
            var (offset, ticks) = Utils.Decode64U(buffer, index);
            uint resource;
            (offset, resource) = Utils.Decode32U(buffer, offset);
            ulong salt;
            (offset, salt) = Utils.Decode64U(buffer, offset);
            ulong counter;
            (offset, counter) = Utils.Decode64U(buffer, offset);

            var token = new HashCash
            {
                dt = new DateTime((long)ticks),
                resource = (int)resource,
                salt = salt,
                counter = counter
            };

            return (offset, token);
        }

    }
}