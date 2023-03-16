/*******************************************************
 * Copyright (C) 2021 James Frowen <JamesFrowenDev@gmail.com>
 * 
 * This file is part of JamesFrowen ClientSidePrediction
 * 
 * The code below can not be copied and/or distributed without the express
 * permission of James Frowen
 *******************************************************/

using System.IO;
using System.Linq;
using UnityEngine;

namespace Mirage.Serialization.HuffmanCoding.Training
{
    public class LoadModel
    {
        public static void SaveToFile(string file, HuffmanCodingModel model)
        {
            File.WriteAllText(file, ToJson(model));
        }

        public static HuffmanCodingModel LoadFromFile(string file)
        {
            return FromJson(File.ReadAllText(file));
        }

        public static string ToJson(HuffmanCodingModel model)
        {
            var json = new ModelJson
            {
                Prefixes = model._prefixes.Select(x => new ModelJson.Prefix { Value = x.Value, BitCount = x.BitCount }).ToArray(),
                DecodeTable = model._decodeTable,
                GroupSize = model._groupSize,
                MaxPrefixLength = model._maxPrefixLength
            };

            return JsonUtility.ToJson(json);
        }

        public static HuffmanCodingModel FromJson(string str)
        {
            var json = JsonUtility.FromJson<ModelJson>(str);

            return new HuffmanCodingModel(
                prefixes: json.Prefixes.Select(x => new Prefix(x.Value, x.BitCount)).ToArray(),
                json.DecodeTable,
                json.MaxPrefixLength,
                json.GroupSize);
        }

        [System.Serializable]
        public struct ModelJson
        {
            public Prefix[] Prefixes;
            public int[] DecodeTable;
            public int MaxPrefixLength;
            public int GroupSize;

            [System.Serializable]
            public struct Prefix
            {
                public uint Value;
                public int BitCount;
            }
        }
    }
}
