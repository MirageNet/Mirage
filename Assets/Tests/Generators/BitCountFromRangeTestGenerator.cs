using JamesFrowen.SimpleCodeGen;
using UnityEditor;
using UnityEngine;

namespace Mirage.Tests.CodeGenerators
{
    public static class BitCountFromRangeTestGenerator
    {
        [MenuItem("Tests Generators/BitCountFromRange")]
        public static void CreateAll()
        {
            var fromTemplate = new CreateFromTemplate("./Assets/Tests/Generators/.BitCountFromRangeTestTemplate.cs");
            Create(fromTemplate, "int", -10, 10, 5, "-3");
            Create(fromTemplate, "int", -20_000, 20_000, 16);
            Create(fromTemplate, "int", -1_000, 0, 10, "-3");
            Create(fromTemplate, "int", -2000, -1000, 10, "-1400");
            Create(fromTemplate, "int", int.MinValue, int.MaxValue, 32);
            Create(fromTemplate, "int", int.MinValue, int.MaxValue, 32, int.MinValue.ToString(), extraName: "min");
            Create(fromTemplate, "int", int.MinValue, int.MaxValue, 32, int.MaxValue.ToString(), extraName: "max");
            Create(fromTemplate, "uint", 0, 5000, 13);
            Create(fromTemplate, "short", -10, 10, 5, "3");
            Create(fromTemplate, "short", -1000, 1000, 11);
            Create(fromTemplate, "short", short.MinValue, short.MaxValue, 16);
            Create(fromTemplate, "ushort", ushort.MinValue, ushort.MaxValue, 16);
            Create(fromTemplate, "MyDirection", -1, 1, 2, "(MyDirection)1",
    @"[System.Serializable]
    public enum MyDirection
    {
        Left = -1,
        None = 0,
        Right = 1,
    }");

            Create(fromTemplate, "MyByteEnum", 0, 3, 2, "(MyByteEnum)3",
    @"[System.Serializable]
    public enum MyByteEnum : byte
    {
        None = 0,
        Slow = 1,
        Fast = 2,
        ReallyFast = 3,
    }");

            AssetDatabase.Refresh();
        }

        private static void Create(CreateFromTemplate fromTemplate, string type, int min, int max, int expectedBitCount, string extraValue = "20", string extraType = "", string extraName = "")
        {
            fromTemplate.Replace("%%MIN%%", min);
            fromTemplate.Replace("%%MAX%%", max);
            fromTemplate.Replace("%%BIT_COUNT%%", expectedBitCount);
            fromTemplate.Replace("%%TYPE%%", type);
            fromTemplate.Replace("%%PAYLOAD_SIZE%%", Mathf.CeilToInt(expectedBitCount / 8f));
            fromTemplate.Replace("%%EXTRA_TYPE%%", extraType);
            fromTemplate.Replace("%%EXTRA_VALUE%%", extraValue);

            string minString = min.ToString().Replace('-', 'N');
            string maxString = max.ToString().Replace('-', 'N');
            string name = $"{type}_{minString}_{maxString}{extraName}";
            fromTemplate.Replace("%%NAME%%", name);

            fromTemplate.WriteToFile($"./Assets/Tests/Generated/BitCountFromRangeTests/BitCountBehaviour_{name}.cs");
        }
    }
}
