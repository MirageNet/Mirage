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
    public struct Prefix
    {
        public readonly uint Value;
        public readonly int BitCount;

        public Prefix(uint value, int bitCount)
        {
            Value = value;
            BitCount = bitCount;
        }
    }
}
