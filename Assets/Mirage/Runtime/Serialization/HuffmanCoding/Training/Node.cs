/*******************************************************
 * Copyright (C) 2021 James Frowen <JamesFrowenDev@gmail.com>
 * 
 * This file is part of JamesFrowen ClientSidePrediction
 * 
 * The code below can not be copied and/or distributed without the express
 * permission of James Frowen
 *******************************************************/

using System;

namespace Mirage.Serialization.HuffmanCoding.Training
{
    public class Node : IComparable<Node>
    {
        public readonly bool IsLeaf;
        private int _depth;

        public int Depth => _depth;
        public SymbolFrequency Data;
        public Node Parent;
        public Node Left;
        public Node Right;

        public Node(bool isLeaf)
        {
            IsLeaf = isLeaf;
        }

        public Node(Node left, Node right)
        {
            IsLeaf = false;
            ConnectLeft(left);
            ConnectRight(right);

            Data.Frequency = right.Data.Frequency + left.Data.Frequency;
        }

        public void SetDepth(int depth)
        {
            _depth = depth;
            Left?.SetDepth(depth + 1);
            Right?.SetDepth(depth + 1);
        }

        public Node AddParent(Node sibling)
        {
            if (!IsLeaf && (Left == null || Right == null))
                throw new InvalidOperationException("Only leafs can have no children");

            var node = new Node(false)
            {
                // new node has parent and depth of `this`
                // effectively new node replaces `this` and moves `this` to lower depth
                Parent = Parent,
                Right = this,
                Left = sibling,
            };
            ReplaceChild(this, node);

            // store in local so we can compare inside assert later
            var depth = Depth;
            node.SetDepth(depth);

            // make sure that this.depth is now +1
            Assert(Depth == depth + 1);

            Parent = node;
            sibling.Parent = node;
            return node;
        }

        public void ReplaceWith(Node node)
        {
            Parent.ReplaceChild(this, node);
        }

        public void ReplaceChild(Node oldChild, Node newChild)
        {
            if (Left == oldChild)
                Left = newChild;
            else if (Right == oldChild)
                Right = newChild;
            else
                throw new InvalidOperationException("old node was not a child");


            newChild.Parent = this;
            newChild.SetDepth(Depth + 1);
        }


        public Node AddLeft(SymbolFrequency data = default)
        {
            return CreateChild(ref Left, data);
        }

        public Node AddRight(SymbolFrequency data = default)
        {
            return CreateChild(ref Right, data);
        }

        public void ConnectLeft(Node left)
        {
            Connect(ref Left, left);
        }

        public void ConnectRight(Node right)
        {
            Connect(ref Right, right);
        }

        private Node CreateChild(ref Node node, SymbolFrequency data)
        {
            if (node != null)
                throw new InvalidOperationException("node was not null");

            node = new Node(true)
            {
                Data = data,
                Parent = this,
                _depth = Depth + 1
            };
            return node;
        }

        private void Connect(ref Node field, Node newNode)
        {
            if (field != null)
                throw new InvalidOperationException("node was not null");

            newNode.Parent = this;
            newNode.SetDepth(Depth + 1);
            field = newNode;
        }

        private static void Assert(bool condition)
        {
            if (condition)
                return;

            throw new Exception($"Assertion failed");
        }

        public Node GetSibling()
        {
            if (Parent.Left == this)
                return Parent.Right;
            else if (Parent.Right == this)
                return Parent.Left;
            else
                throw new InvalidOperationException("Not a child of parent, Node set up incorrect");
        }

        public int CompareTo(Node other)
        {
            return Data.Frequency - other.Data.Frequency;
        }
    }
}
