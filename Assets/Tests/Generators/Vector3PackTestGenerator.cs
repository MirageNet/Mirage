using JamesFrowen.SimpleCodeGen;
using UnityEditor;
using UnityEngine;

namespace Mirage.Tests.CodeGenerators
{
    public static class Vector3PackTestGenerator
    {
        [MenuItem("Tests Generators/Vector3Pack")]
        public static void CreateAll()
        {
            var fromTemplate = new CreateFromTemplate("./Assets/Tests/Generators/.Vector3PackTestTemplate.txt");
            Create(fromTemplate, "100_28f3", new Vector3(100, 20, 100), "0.2f, 0.2f, 0.2f", new Vector3(10.3f, 0.2f, 20), 0.2f, 28);
            Create(fromTemplate, "100_28f", new Vector3(100, 20, 100), "0.2f", new Vector3(10.3f, 0.2f, 20), 0.2f, 28);
            Create(fromTemplate, "100_28b3", new Vector3(100, 20, 100), "10, 8, 10", new Vector3(-10.3f, 0.2f, 20), 0.2f, 28);
            Create(fromTemplate, "100_30b", new Vector3(100, 100, 100), "10", new Vector3(-10.3f, 0.2f, 20), 0.2f, 30);

            Create(fromTemplate, "1000_42f3", new Vector3(1000, 200, 1000), "0.1f, 0.1f, 0.1f", new Vector3(-10.3f, 0.2f, 20), 0.1f, 42);
            Create(fromTemplate, "1000_42f", new Vector3(1000, 200, 1000), "0.1f", new Vector3(-10.3f, 0.2f, 20), 0.1f, 42);
            Create(fromTemplate, "1000_42b3", new Vector3(1000, 200, 1000), "15, 12, 15", new Vector3(10.3f, 0.2f, 20), 0.2f, 42);
            Create(fromTemplate, "1000_45b", new Vector3(1000, 200, 1000), "15", new Vector3(10.3f, 0.2f, 20), 0.2f, 45);

            Create(fromTemplate, "200_39f3", new Vector3(200, 200, 200), "0.05f, 0.05f, 0.05f", new Vector3(-10.3f, 0.2f, -20), 0.1f, 39);
            Create(fromTemplate, "200_39f", new Vector3(200, 200, 200), "0.05f", new Vector3(-10.3f, 0.2f, -20), 0.1f, 39);
            Create(fromTemplate, "200_39b3", new Vector3(200, 200, 200), "13, 13, 13", new Vector3(10.3f, 0.2f, 20), 0.2f, 39);
            Create(fromTemplate, "200_39b", new Vector3(200, 200, 200), "13", new Vector3(10.3f, 0.2f, 20), 0.2f, 39);

            AssetDatabase.Refresh();
        }

        private static void Create(CreateFromTemplate fromTemplate, string name, Vector3 max, string ArgAttribute2, Vector3 value, float within, int bitcount)
        {
            fromTemplate.Replace("%%NAME%%", name);
            fromTemplate.Replace("%%PACKER_ATTRIBUTE%%", $"{max.x}f, {max.y}f, {max.z}f, {ArgAttribute2}");
            fromTemplate.Replace("%%VALUE%%", $"new Vector3({value.x}f, {value.y}f, {value.z}f)");
            fromTemplate.Replace("%%WITHIN%%", $"{within}f");
            fromTemplate.Replace("%%BIT_COUNT%%", bitcount);

            fromTemplate.WriteToFile($"./Assets/Tests/Generated/Vector3PackTests/Vector3PackBehaviour_{name}.cs");
        }
    }
}
