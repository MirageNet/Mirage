/*******************************************************
 * Copyright (C) 2021 James Frowen <JamesFrowenDev@gmail.com>
 * 
 * This file is part of JamesFrowen ClientSidePrediction
 * 
 * The code below can not be copied and/or distributed without the express
 * permission of James Frowen
 *******************************************************/

namespace Mirage.Serialization.HuffmanCoding.Training
{
    public struct SymbolFrequency
    {
        /// <summary>
        /// what bucket value this is for
        /// </summary>
        public int Symbol;
        public int Frequency;

        public void Deconstruct(out int index, out int count)
        {
            index = Symbol;
            count = Frequency;
        }
    }
}
