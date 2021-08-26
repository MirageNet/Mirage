using JamesFrowen.SimpleCodeGen;
using UnityEditor;

namespace Mirage.Tests.CodeGenerators
{
    public static class FloatPackTestGenerator
    {
        [MenuItem("Tests Generators/FloatPack")]
        public static void CreateAll()
        {
            var fromTemplate = new CreateFromTemplate("./Assets/Tests/Generators/.FloatPackTestTemplate.txt");
            Create(fromTemplate, 100f, "0.2f", 5.2f, 0.196f, 10);
            Create(fromTemplate, 100f, "0.02f", 5.2f, 0.0123f, 14);

            Create(fromTemplate, 500f, "0.1f", 5.2f, 0.0123f, 14);
            Create(fromTemplate, 500f, "0.01f", 5.2f, 0.00763f, 17);

            Create(fromTemplate, 1f, "8", 0.2f, 0.00785f, 8);
            Create(fromTemplate, 10f, "10", 3.3f, 0.0191f, 10);

            AssetDatabase.Refresh();
        }

        private static void Create(CreateFromTemplate fromTemplate, float max, string ArgAttribute2, float value, float within, int bitcount)
        {
            string name = $"{max}_{bitcount}";
            fromTemplate.Replace("%%NAME%%", name);
            fromTemplate.Replace("%%PACKER_ATTRIBUTE%%", $"{max}, {ArgAttribute2}");
            fromTemplate.Replace("%%VALUE%%", $"{value}f");
            fromTemplate.Replace("%%WITHIN%%", $"{within}f");
            fromTemplate.Replace("%%BIT_COUNT%%", bitcount);

            fromTemplate.WriteToFile($"./Assets/Tests/Generated/FloatPackTests/FloatPackBehaviour_{name}.cs");
        }
    }
}
