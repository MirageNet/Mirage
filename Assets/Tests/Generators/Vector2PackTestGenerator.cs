using JamesFrowen.SimpleCodeGen;
using UnityEditor;
using UnityEngine;

namespace Mirage.Tests.CodeGenerators
{
    public static class Vector2PackTestGenerator
    {
        [MenuItem("Tests Generators/Vector2Pack")]
        public static void CreateAll()
        {
            var fromTemplate = new CreateFromTemplate("./Assets/Tests/Generators/.Vector2PackTestTemplate.txt");
            Create(fromTemplate, "100_28f3", new Vector2(100, 20), "0.2f, 0.2f", new Vector2(10.3f, 0.2f), 0.2f, 18);
            Create(fromTemplate, "100_28f", new Vector2(100, 20), "0.2f", new Vector2(10.3f, 0.2f), 0.2f, 18);
            Create(fromTemplate, "100_28b3", new Vector2(100, 20), "10, 8", new Vector2(-10.3f, 0.2f), 0.2f, 18);
            Create(fromTemplate, "100_30f", new Vector2(100, 100), "10", new Vector2(-10.3f, 0.2f), 0.2f, 20);

            Create(fromTemplate, "1000_42f3", new Vector2(1000, 200), "0.1f, 0.1f", new Vector2(-10.3f, 0.2f), 0.1f, 27);
            Create(fromTemplate, "1000_42f", new Vector2(1000, 200), "0.1f", new Vector2(-10.3f, 0.2f), 0.1f, 27);
            Create(fromTemplate, "1000_42b3", new Vector2(1000, 200), "15, 12", new Vector2(10.3f, 0.2f), 0.2f, 27);
            Create(fromTemplate, "1000_45f", new Vector2(1000, 200), "15", new Vector2(10.3f, 0.2f), 0.2f, 30);

            Create(fromTemplate, "200_39f3", new Vector2(200, 200), "0.05f, 0.05f", new Vector2(-10.3f, 0.2f), 0.1f, 26);
            Create(fromTemplate, "200_39f", new Vector2(200, 200), "0.05f", new Vector2(-10.3f, 0.2f), 0.1f, 26);
            Create(fromTemplate, "200_39b3", new Vector2(200, 200), "13, 13", new Vector2(10.3f, 0.2f), 0.2f, 26);
            Create(fromTemplate, "200_39f", new Vector2(200, 200), "13", new Vector2(10.3f, 0.2f), 0.2f, 26);

            AssetDatabase.Refresh();
        }

        private static void Create(CreateFromTemplate fromTemplate, string name, Vector2 max, string ArgAttribute2, Vector2 value, float within, int bitcount)
        {
            fromTemplate.Replace("%%NAME%%", name);
            fromTemplate.Replace("%%PACKER_ATTRIBUTE%%", $"{max.x}f, {max.y}f, {ArgAttribute2}");
            fromTemplate.Replace("%%VALUE%%", $"new Vector2({value.x}f, {value.y}f)");
            fromTemplate.Replace("%%WITHIN%%", $"{within}f");
            fromTemplate.Replace("%%BIT_COUNT%%", bitcount);

            fromTemplate.WriteToFile($"./Assets/Tests/Generated/Vector2PackTests/Vector2PackBehaviour_{name}.cs");
        }
    }
}
