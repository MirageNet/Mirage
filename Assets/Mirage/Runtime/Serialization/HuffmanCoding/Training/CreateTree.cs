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
    public static unsafe class CreateTree
    {
        public static Tree Create(List<SymbolFrequency> frequencyData, int? maxDepth)
        {
            // todo is there another way to check depth of tree before we generate it?
            //      might not be a problem as creating the tree itself is quick

            var noLimitTree = CreateNoLimit(frequencyData);
            // if no limit, then no extra stuff to check, so return early
            if (!maxDepth.HasValue)
                return noLimitTree;

            var depth = noLimitTree.GetMaxDepth();
            if (depth <= maxDepth.Value)
                return noLimitTree;

            // if depth of best tree is to much, then create depth Limit one instaed
            return CreateDepthLimited(frequencyData, maxDepth.Value);
        }

        /// <summary>
        /// Normal huffman tree algorith, will have optimal bandwidth for <paramref name="frequencyData"/>
        /// <para>to limit the depth of the tree use <see cref="CreateDepthLimited"/> instead</para>
        /// </summary>
        /// <param name="frequencyData"></param>
        /// <returns></returns>
        private static Tree CreateNoLimit(List<SymbolFrequency> frequencyData)
        {
            var leaves = new List<Node>();

            frequencyData.Sort((x, y) => x.Frequency.CompareTo(y.Frequency));

            var unconnectedNodes = new List<Node>();
            foreach (var data in frequencyData)
            {
                var leaf = new Node(true);
                leaf.Data = data;
                leaves.Add(leaf);
                unconnectedNodes.Add(leaf);
            }

            while (unconnectedNodes.Count > 1)
            {
                unconnectedNodes.Sort();

                // take smallest 2 and connect
                var right = unconnectedNodes[0];
                var left = unconnectedNodes[1];

                var parent = new Node(left, right);


                unconnectedNodes.RemoveRange(0, 2);
                unconnectedNodes.Add(parent);
            }

            var root = unconnectedNodes[0];
            return new Tree(root, leaves.ToArray());
        }

        /// <summary>
        /// Created depth limit huffman tree using <see cref="PackageMerge"/>
        /// <para>
        /// May not run correctly if <paramref name="maxDepth"/> is too high for given <paramref name="frequencyData"/>.
        /// </para>
        /// </summary>
        /// <param name="frequencyData"></param>
        /// <param name="maxDepth"></param>
        /// <returns></returns>
        public static Tree CreateDepthLimited(List<SymbolFrequency> frequencyData, int maxDepth)
        {
            var leaves = frequencyData.Select(x => new Node(true) { Data = x }).ToArray();

            var codeLengths = PackageMerge.GetCodeLengths(maxDepth, leaves);

            var leafLengths = new (Node leaf, int codeLength)[codeLengths.Length];
            for (var i = 0; i < codeLengths.Length; i++)
            {
                leafLengths[i] = (leaves[i], codeLengths[i]);
            }

            var root = CreateFromDepths(maxDepth, leafLengths);
            return new Tree(root, leaves);
        }

        private static Node CreateFromDepths(int maxDepth, (Node leaf, int codeLength)[] leafLengths)
        {
            // merge nodes from each depth moving up
            // by the time we get to depth =1 we should have 1 pair left, (their parent will be the root)
            var nodesAtDepth = new List<Node>();
            for (var depth = maxDepth; depth > 0; depth--)
            {
                // add new nodes that are at current depth
                // note: nodesAtDepth will include parent nodes from previous loops
                foreach (var leafLength in leafLengths)
                {
                    if (leafLength.codeLength == depth)
                        nodesAtDepth.Add(leafLength.leaf);
                }

                // there should always be an even number of nodes at each level,
                // if not then something has gone wrong
                if (nodesAtDepth.Count % 2 == 1)
                    throw new Exception("Failed to find pair");

                var newNodes = nodesAtDepth.Count / 2;
                for (var i = 0; i < newNodes; i++)
                {
                    var right = nodesAtDepth[i * 2];
                    var left = nodesAtDepth[(i * 2) + 1];
                    var parent = new Node(left, right);
                    // add new nodes to start of list (it is safe to overwrite here)
                    nodesAtDepth[i] = parent;
                }

                // remove nodes that are not new
                nodesAtDepth.RemoveRange(newNodes, nodesAtDepth.Count - newNodes);
            }


            // after all loops we should be left with the root node (depth=0)

            if (nodesAtDepth.Count != 1)
                throw new Exception("Failed to create root");

            return nodesAtDepth[0];
        }
    }
}
