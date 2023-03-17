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
using System.IO;

namespace Mirage.Serialization.HuffmanCoding.Training
{
    public unsafe class Debugging
    {
        private static List<byte[]> _rawCache;
        private static List<byte[]> _zeroPackCache;
        private static readonly Random random = new Random();

        public static List<byte[]> CreateRaw()
        {
            if (_rawCache != null && _rawCache.Count != 0)
                return _rawCache;


            var frameCount = 100;
            var rawFrames = new List<byte[]>(frameCount);
            for (var i = 1; i <= frameCount; i++)
            {
                var raw = new byte[10_000];
                fixed (byte* ptr = &raw[0])
                {
                    FillWithRandom((int*)ptr, 2500);
                }
                rawFrames.Add(raw);

                if (raw.Length % 4 != 0)
                    throw new Exception($"total bytes was not multiple of 4");
            }
            _rawCache = rawFrames;
            return _rawCache;
        }

        private static void FillWithRandom(int* raw, int count)
        {
            // different groups, so that numbers are not just pure random, but weighted instead

            for (var i = 0; i < count; i++)
            {
                var group = random.Next(0, 100);
                if (group < 10)
                {
                    raw[i] = 0;
                }
                else if (group < 50)
                {
                    raw[i] = random.Next(1, 1 << 4);
                }
                else if (group < 60)
                {
                    raw[i] = random.Next(1 << 4, 1 << 6);
                }
                else if (group < 70)
                {
                    raw[i] = random.Next(1 << 6, 1 << 12);
                }
                else if (group < 80)
                {
                    raw[i] = random.Next(1 << 12, 1 << 20);
                }
                else
                {
                    raw[i] = random.Next(1 << 20, 1 << 30);
                }

                var sign = random.Next(0, 1);
                if (sign == 1)
                    raw[i] = -raw[i];
            }
        }

        public static List<byte[]> LoadRaw(string pathFormat)
        {
            if (_rawCache != null && _rawCache.Count != 0)
                return _rawCache;


            var frameCount = 100;
            var rawFrames = new List<byte[]>(frameCount);
            for (var i = 1; i <= frameCount; i++)
            {
                var path = string.Format(pathFormat, i);
                if (!File.Exists(path))
                    continue;
                var raw = File.ReadAllBytes(path);
                rawFrames.Add(raw);

                if (raw.Length % 4 != 0)
                    throw new Exception($"total bytes was not multiple of 4");
            }
            _rawCache = rawFrames;
            return _rawCache;
        }
        public static byte[] MergeRaw(List<byte[]> rawFrames)
        {
            var totalBytes = 0;
            for (var i = 0; i < rawFrames.Count; i++)
                totalBytes += rawFrames[i].Length;

            var mergedRaw = new byte[totalBytes];
            var offset = 0;
            for (var i = 0; i < rawFrames.Count; i++)
            {
                Buffer.BlockCopy(rawFrames[i], 0, mergedRaw, offset, rawFrames[i].Length);
                offset += rawFrames[i].Length;
            }
            return mergedRaw;
        }

        /// <summary>
        /// Walks the tree and logs the data
        /// </summary>
        /// <param name="node"></param>
        /// <param name="prefix"></param>
        public static void Walk(Tree tree)
        {
            tree.Walk((node, prefix, depth) =>
            {
                if (depth == 0)
                    Log($"---Tree---");

                if (node.IsLeaf)
                {
                    var (symbol, count) = node.Data;
                    var prefixStr = Convert.ToString(prefix, 2).PadLeft(depth, '0');
                    Log($"Symbol:{symbol:D2}, count:{count}, prefix:{prefixStr}");
                }
            });

            void Log(string str)
            {
#if !BIT_PACKING_NO_DEBUG
                UnityEngine.Debug.Log(str);
#else
                Console.WriteLine(str);
#endif
            }
        }

        /// <summary>
        /// Using delta to write zero count instead of each zero
        /// </summary>
        /// <returns></returns>
        public static List<byte[]> RawZeroPacked(string pathFormat)
        {
            if (_zeroPackCache != null && _zeroPackCache.Count != 0)
                return _zeroPackCache;

            var raw = LoadRaw(pathFormat);
            var deltaRaw = new List<byte[]>(raw.Count);

            var delta = new DeltaSnapshot_ValueZeroCounts();
            var writer = new DebugWriter();
            var zero = new byte[0];
            for (var i = 0; i < raw.Count; i++)
            {
                var current = raw[i];
                // if i is 0, then set null
                var previous = i > 0 ? raw[i - 1] : null;
                // if size is different set null
                if (previous != null && previous.Length != current.Length)
                    previous = null;

                // if null, then use zero buffer
                if (previous == null)
                {
                    // resize if needed
                    if (zero.Length < current.Length)
                        Array.Resize(ref zero, current.Length);

                    previous = zero;
                }

                fixed (byte* to = &current[0])
                {
                    fixed (byte* from = &previous[0])
                    {
                        delta.WriteDelta(writer, current.Length / 4, (int*)from, (int*)to);
                    }
                }

                deltaRaw.Add(writer.Flush());
            }

            _zeroPackCache = deltaRaw;
            return _zeroPackCache;
        }

        public class DebugWriter
        {
            private int position;
            private uint[] _buffer = new uint[1200];

            public void Write(int value)
            {
                Write(ZigZag.Encode(value));
            }
            public void Write(uint value)
            {
                if (position >= _buffer.Length)
                    Array.Resize(ref _buffer, _buffer.Length * 2);

                _buffer[position] = value;
                position++;
            }

            public byte[] Flush()
            {
                var array = new byte[position * 4];
                Buffer.BlockCopy(_buffer, 0, array, 0, position * 4);
                position = 0;
                return array;
            }
        }

        public unsafe class DeltaSnapshot_ValueZeroCounts
        {
            public void WriteDelta(DebugWriter writer, int intSize, int* from, int* to)
            {
                // writes number of zero/values before there is a opposite 
                uint zeroCount = 0;
                // first is likely to be zero for delta because it will be netid that is unchanging
                var countingZeros = true;

                var diff = stackalloc int[intSize];
                // write all zeros counts at start of message, then do 2nd loop to write values
                for (var i = 0; i < intSize; i++)
                {
                    diff[i] = to[i] - from[i];
                    var diffZero = diff[i] == 0;

                    if (countingZeros == diffZero)
                        zeroCount++;
                    else
                    {
                        writer.Write(zeroCount);
                        zeroCount = 1;
                        countingZeros = !countingZeros;
                    }
                }

                if (zeroCount > 0)
                    // write how many zeros we saw 
                    writer.Write(zeroCount);

                for (var i = 0; i < intSize; i++)
                {
                    var diffZero = diff[i] == 0;

                    if (!diffZero)
                        writer.Write(diff[i]);
                }
            }
        }
    }
}
