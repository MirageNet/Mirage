/*******************************************************
 * Copyright (C) 2021 James Frowen <JamesFrowenDev@gmail.com>
 * 
 * This file is part of JamesFrowen ClientSidePrediction
 * 
 * The code below can not be copied and/or distributed without the express
 * permission of James Frowen
 *******************************************************/

using System;
using System.Linq;

namespace Mirage.Serialization.HuffmanCoding.Training
{
    /// <summary>
    /// Packake Merge is an algorithm to help create length limited huffman trees.
    /// <para>
    /// Resources to help understand:<br/>
    /// - <see href="https://experiencestack.co/length-limited-huffman-codes-21971f021d43">Length-Limited Huffman Codes</see><br/>
    /// - <see href="https://create.stephan-brumme.com/length-limited-prefix-codes/">Length-Limited Prefix Codes</see>
    /// </para>
    /// </summary>
    public static class PackageMerge
    {
        /// <summary>
        /// Runs Package Merge algorithm <paramref name="maxDepth"/> times in order to get the code lengths for each leave for huffman tree
        /// </summary>
        /// <param name="maxDepth"></param>
        /// <param name="leaves"></param>
        /// <returns></returns>
        public static int[] GetCodeLengths(int maxDepth, Node[] leaves)
        {
            var itterations = GetAllItterations(maxDepth, leaves);

            var codeLength = new int[leaves.Length];

            // total nodes excluding root, property of biary tree
            var checkCount = leaves.Length * 2 - 2;
            for (var ittr = itterations.Length - 1; ittr >= 0; ittr--)
            {
                var itteration = itterations[ittr];

                var symbol = 0;
                var numMerged = 0;

                for (var i = 0; i < checkCount; i++)
                {
                    var node = itteration[i];
                    if (node.IsLeaf)
                    {
                        codeLength[symbol]++;
                        symbol++;
                    }
                    else
                    {
                        numMerged++;
                    }
                }

                // set count for next loop
                checkCount = numMerged * 2;
            }

            return codeLength;
        }

        /// <summary>
        /// Creates <paramref name="maxDepth"/> itterations, where 0th itteration is just leaves
        /// </summary>
        /// <param name="maxDepth"></param>
        /// <param name="leaves"></param>
        /// <returns></returns>
        private static A[][] GetAllItterations(int maxDepth, Node[] leaves)
        {
            // make sure leaves are sorted
            Array.Sort(leaves);

            var all = new A[maxDepth][];
            all[0] = leaves.Select(x => new A(x.Data.Frequency)).ToArray();
            for (var i = 1; i < maxDepth; i++)
            {
                all[i] = GetNextItteration(all[0], all[i - 1]);
            }
            return all;
        }

        /// <summary>
        /// Creates next itteration by taking the sum of pairs (from smallest to largest), Combine with the list of leaves
        /// </summary>
        /// <param name="leaves"></param>
        /// <param name="previous"></param>
        /// <returns></returns>
        private static A[] GetNextItteration(A[] leaves, A[] previous)
        {
            var newNodes = previous.Length / 2;
            var nodes = new A[leaves.Length + newNodes];

            // merge pairs, (ignoring largest if odd count)
            for (var i = 0; i < newNodes; i++)
            {
                var right = previous[i * 2];
                var left = previous[i * 2 + 1];

                var parent = new A(left, right);
                // just overwrite lower index, we dont need them any more in this loop
                nodes[i] = parent;
            }

            // add leaves to end of new pairs
            for (var i = 0; i < leaves.Length; i++)
            {
                nodes[newNodes + i] = leaves[i];
            }

            Array.Sort(nodes);

            return nodes;
        }

        /// <summary>
        /// Data structs to hold Frequency and say if an item is a leaf or a sum of pairs
        /// </summary>
        private struct A : IComparable<A>
        {
            public readonly int Frequency;
            public readonly bool IsLeaf;

            public A(int frequency)
            {
                Frequency = frequency;
                IsLeaf = true;
            }

            public A(A left, A right)
            {
                Frequency = left.Frequency + right.Frequency;
                IsLeaf = false;
            }

            public int CompareTo(A other)
            {
                return Frequency - other.Frequency;
            }
        }
    }
}
