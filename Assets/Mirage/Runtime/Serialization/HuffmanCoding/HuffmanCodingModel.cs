/*******************************************************
 * Copyright (C) 2021 James Frowen <JamesFrowenDev@gmail.com>
 * 
 * This file is part of JamesFrowen ClientSidePrediction
 * 
 * The code below can not be copied and/or distributed without the express
 * permission of James Frowen
 *******************************************************/

namespace Mirage.Serialization.HuffmanCoding
{
    public class HuffmanCodingModel : ICompressInt
    {
        internal readonly Prefix[] _prefixes;
        internal readonly int[] _decodeTable;
        internal readonly int _maxPrefixLength;
        internal readonly int _groupSize;

        public HuffmanCodingModel(Prefix[] prefixes, int[] decodeTable, int maxPrefixLength, int groupSize)
        {
            _prefixes = prefixes;
            _decodeTable = decodeTable;
            _maxPrefixLength = maxPrefixLength;
            _groupSize = groupSize;
        }

        public void Write(NetworkWriter writer, uint value)
        {
            var bucket = GetBucketFromValue(value);

            var prefix = _prefixes[bucket];
            var bitCount = GetBitCount(bucket);

            writer.Write(prefix.Value, prefix.BitCount);
            writer.Write(value, bitCount);
        }
        public uint Read(NetworkReader reader)
        {
            var decodeKey = reader.Peak(_maxPrefixLength);
            var bucket = _decodeTable[decodeKey];//
            var bitCount = GetBitCount(bucket);

            var prefix = _prefixes[bucket];
            reader.Skip(prefix.BitCount);
            return (uint)reader.Read(bitCount);
        }

        private int GetBucket(int bitCount)
        {
            // gets bucket index from bitcount
            // will round up it next groupSize, but keep 0 by itself
            // eg _groupSize=2,
            // bucket[0] = 0 bits
            // bucket[1] = 1 bits or 2 bits
            // bucket[2] = 3 bits or 4 bits
            return (bitCount + _groupSize - 1) / _groupSize;
        }
        private int GetBucketFromValue(uint value)
        {
            var bitCount = BitHelper.BitCount(value);
            return GetBucket(bitCount);
        }
        private int GetBitCount(int bucket)
        {
            // eg _groupSize=2,
            // bucket 0 => 0
            // bucket 1 => 2 bits
            // bucket 2 => 4 bits
            // bucket 3 => 6 bits
            return bucket * _groupSize;
        }

    }
}
