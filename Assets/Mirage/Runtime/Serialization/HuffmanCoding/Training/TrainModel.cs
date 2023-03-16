/*******************************************************
 * Copyright (C) 2021 James Frowen <JamesFrowenDev@gmail.com>
 * 
 * This file is part of JamesFrowen ClientSidePrediction
 * 
 * The code below can not be copied and/or distributed without the express
 * permission of James Frowen
 *******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;

namespace Mirage.Serialization.HuffmanCoding.Training
{
    public class TrainModel
    {
        public static HuffmanCodingModel Train(int groupSize, List<byte[]> raw)
        {
            var data = CreateFrequencyData(groupSize, raw);
            var tree = CreateTree.Create(data, 6);
            return CreateModel(groupSize, tree);
        }


        protected static int BucketCount(int groupSize) => (33 / groupSize) + 1;
        protected static int SymbolSize(int groupSize, int symbol)
        {
            return symbol * groupSize;
        }

        public static HuffmanCodingModel CreateModel(int groupSize, Tree tree)
        {
            var prefixes = new Prefix[BucketCount(groupSize)];
            var maxPrefixLength = 0;

            tree.Walk((node, prefix, depth) =>
            {
                if (node.IsLeaf)
                {
                    prefixes[node.Data.Symbol] = new Prefix(prefix, depth);
                    maxPrefixLength = Math.Max(maxPrefixLength, depth);
                }
            });


            var decodeTable = new int[1 << maxPrefixLength];
            for (var decode = 0; decode < decodeTable.Length; decode++)
            {
                decodeTable[decode] = CalcualteDecodeBucket(prefixes, decode);
            }

            //for (var i = 0; i < BucketCount; i++)
            //{
            //    Log($"Bucket:{i} " +
            //        $"Size:{SymbolSize(i):D2}, " +
            //        $"frequency:{tree.Leaves.First(x => x.Data.Symbol == i).Data.Frequency}, " +
            //        $"count:{_prefixes[i].BitCount}, " +
            //        $"prefix:{Convert.ToString(_prefixes[i].Value, 2).PadLeft(_prefixes[i].BitCount, '0')}");
            //}

            return new HuffmanCodingModel(prefixes, decodeTable, maxPrefixLength, groupSize);
        }

        private static int CalcualteDecodeBucket(Prefix[] prefixes, int decode)
        {
            for (var bucket = 0; bucket < prefixes.Length; bucket++)
            {
                var prefixLength = prefixes[bucket].BitCount;

                // create a mask for just the prefix length
                // extra written bits will be part of value itself and not prefix
                // because prefix values are unique this should always find correct bucket

                var masked = decode & (int)BitMask.Mask(prefixLength);
                if (prefixes[bucket].Value == masked)
                    return bucket;
            }

            throw new Exception($"Decode not found");
        }



        public static List<SymbolFrequency> CreateFrequencyData(int groupSize, List<byte[]> rawFrames)
        {
            var frequencies = new int[33];
            CountSizes(rawFrames, frequencies);
            var frequencyData = Group(groupSize, frequencies);

#if !BIT_PACKING_NO_DEBUG
            foreach (var pair in frequencyData)
            {
                var bits = pair.Symbol;
                var count = pair.Frequency;
                UnityEngine.Debug.Log($"Size:{bits:D2}, Count:{count}");
            }
#endif

            return frequencyData;
        }

        private static void CountSizes(List<byte[]> rawFrames, int[] outFrequency)
        {
            foreach (var frame in rawFrames)
            {
                CountSizes(frame, outFrequency);
            }
        }
        private static unsafe int[] CountSizes(byte[] raw, int[] outFrequency)
        {
            fixed (byte* bPtr = &raw[0])
            {
                var ptr = (int*)bPtr;
                CountSizes(ptr, raw.Length / 4, outFrequency);
            }

            return outFrequency;
        }

        private static unsafe void CountSizes(int* ptr, int intCount, int[] outFrequency)
        {
            for (var i = 0; i < intCount; i++)
            {
                var value = ptr[i];
                var zigzag = ZigZag.Encode(value);

                if (zigzag == 0)
                    outFrequency[0]++;
                else
                {
                    var count = BitHelper.BitCount(zigzag);
                    outFrequency[count]++;
                }
            }
        }

        private static List<SymbolFrequency> Group(int groupSize, int[] frequencies)
        {
            var frequencyDictionary = new Dictionary<int, int>();
            var groupSizeMinusOne = groupSize - 1;
            for (var bits = 0; bits < 33; bits++)
            {
                var count = frequencies[bits];
                //Debug.Log($"Size:{bits:D2}, Count:{count}");

                var bucket = (bits + groupSizeMinusOne) / groupSize;
                if (!frequencyDictionary.ContainsKey(bucket))
                    frequencyDictionary.Add(bucket, 0);

                frequencyDictionary[bucket] += count;
            }

            return frequencyDictionary.Select(x => new SymbolFrequency { Symbol = x.Key, Frequency = x.Value }).ToList();
        }

        public static void PrintDecodeTable(HuffmanCodingModel model)
        {
            Console.WriteLine($"Decode Table, Length:{model._decodeTable.Length}");

            for (var decode = 0; decode < model._decodeTable.Length; decode++)
            {
                Console.WriteLine($"Decode:{decode} = {model._decodeTable[decode]}");
            }
        }

        protected static void Log(string str)
        {
#if !BIT_PACKING_NO_DEBUG
            UnityEngine.Debug.Log(str);
#else
            Console.WriteLine(str);
#endif
        }
    }
}
