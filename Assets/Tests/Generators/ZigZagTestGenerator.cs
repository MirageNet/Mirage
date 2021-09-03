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
            Create(fromTemplate, "short", extraValue: "15");
            Create(fromTemplate, "short", extraValue: "-20", extraName: "_negative");
            Create(fromTemplate, "int", extraValue: "100");
            Create(fromTemplate, "int", extraValue: "-25", extraName: "_negative");
            Create(fromTemplate, "long", extraValue: "14");
            Create(fromTemplate, "long", extraValue: "-30", extraName: "_negative");

            Create(fromTemplate, "MyEnum", 4, "(MyEnum)1",
  @"[System.Serializable]
    public enum MyEnum
    {
        Negative = -1,
        Zero = 0,
        Positive = 1,
    }");
            Create(fromTemplate, "MyEnum2", 4, "(MyEnum2)(-1)",
  @"[System.Serializable]
    public enum MyEnum2
    {
        Negative = -1,
        Zero = 0,
        Positive = 1,
    }", extraName: "_negative");

            AssetDatabase.Refresh();
        }

        private static void Create(CreateFromTemplate fromTemplate, string type, int bitCount = 10, string extraValue = "20", string extraType = "", string extraName = "")
        {
            fromTemplate.Replace("%%BIT_COUNT%%", bitCount);
            fromTemplate.Replace("%%TYPE%%", type);
            fromTemplate.Replace("%%PAYLOAD_SIZE%%", Mathf.CeilToInt(bitCount / 8f));
            fromTemplate.Replace("%%EXTRA_TYPE%%", extraType);
            fromTemplate.Replace("%%EXTRA_VALUE%%", extraValue);
            fromTemplate.Replace("%%EXTRA_NAME%%", extraName);

            fromTemplate.WriteToFile($"./Assets/Tests/Generated/ZigZagTests/ZigZagBehaviour_{type}_{bitCount}{extraName}.cs");
        }
    }
}
