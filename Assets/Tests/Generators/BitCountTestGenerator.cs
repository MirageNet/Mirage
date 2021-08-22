using JamesFrowen.SimpleCodeGen;
using UnityEditor;
using UnityEngine;

namespace Mirage.Tests.CodeGenerators
{
    public static class BitCountTestGenerator
    {
        [MenuItem("Tests Generators/BitCount")]
        public static void CreateAll()
        {
            var fromTemplate = new CreateFromTemplate("./Assets/Tests/Generators/.BitCountTestTemplate.txt");
            Create(fromTemplate, "int", 10);
            Create(fromTemplate, "int", 17);
            Create(fromTemplate, "int", 32);
            Create(fromTemplate, "short", 4);
            Create(fromTemplate, "short", 12);
            Create(fromTemplate, "ulong", 5);
            Create(fromTemplate, "ulong", 24);
            Create(fromTemplate, "ulong", 64);

            AssetDatabase.Refresh();
        }

        private static void Create(CreateFromTemplate fromTemplate, string type, int bitCount)
        {
            fromTemplate.Replace("%%BIT_COUNT%%", bitCount);
            fromTemplate.Replace("%%TYPE%%", type);
            fromTemplate.Replace("%%PAYLOAD_SIZE%%", Mathf.CeilToInt(bitCount / 8f));
            fromTemplate.WriteToFile($"./Assets/Tests/Generated/BitCountTests/BitCountBehaviour_{type}_{bitCount}.cs");
        }
    }
}
