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
    public class Tree
    {
        public readonly Node[] Leaves;
        public readonly Node Root;

        public Tree(Node root, Node[] leaves)
        {
            Root = root;
            Leaves = leaves;
        }

        public int GetMaxDepth()
        {
            return Leaves.Max(x => x.Depth);
        }

        public void Walk(Action<Node, uint, int> extraAction, uint prefix = 0, int depth = 0)
        {
            Walk(Root, extraAction, prefix, depth);
        }

        public void Walk(Node node, Action<Node, uint, int> extraAction, uint prefix = 0, int depth = 0)
        {
            //var lStr = node.IsLeaf ? $"leaf, Bucket:{node.Index}" : "";
            //Console.WriteLine($"{depth:D2},{Convert.ToString(prefix, 2).PadLeft(depth, '0').PadRight(20)} {lStr} ");

            extraAction?.Invoke(node, prefix, depth);

            if (!node.IsLeaf)
            {
                Walk(node.Left, extraAction, prefix, depth + 1);
                Walk(node.Right, extraAction, prefix | (1u << depth), depth + 1);
            }
        }
    }
}
