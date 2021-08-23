using JamesFrowen.SimpleCodeGen;
using UnityEditor;
using UnityEngine;

namespace Mirage.Tests.CodeGenerators
{
    public static class ZigZagTestGenerator
    {
        [MenuItem("Tests Generators/ZigZag")]
        public static void CreateAll()
        {
            var fromTemplate = new CreateFromTemplate("./Assets/Tests/Generators/.ZigZagTestTemplate.txt");
            Create(fromTemplate, "short", extraValue: "-20");
            Create(fromTemplate, "int", extraValue: "-25");
            Create(fromTemplate, "long", extraValue: "-30");
            Create(fromTemplate, "MyEnum", 4, "(MyEnum)3",
    @"[System.Serializable]
    public enum MyEnum
    {
        Negative = -1,
        Zero = 0,
        Positive = 1,
    }");
            AssetDatabase.Refresh();
        }

        private static void Create(CreateFromTemplate fromTemplate, string type, int bitCount = 10, string extraValue = "20", string extraType = "")
        {
            fromTemplate.Replace("%%BIT_COUNT%%", bitCount);
            fromTemplate.Replace("%%TYPE%%", type);
            fromTemplate.Replace("%%PAYLOAD_SIZE%%", Mathf.CeilToInt(bitCount / 8f));
            fromTemplate.Replace("%%EXTRA_TYPE%%", extraType);
            fromTemplate.Replace("%%EXTRA_VALUE%%", extraValue);

            fromTemplate.WriteToFile($"./Assets/Tests/Generated/ZigZagTests/ZigZagBehaviour_{type}_{bitCount}.cs");
        }
    }
}
