using JamesFrowen.SimpleCodeGen;
using UnityEditor;
using UnityEngine;

namespace Mirage.Tests.CodeGenerators
{
    public static class QuaternionPackTestGenerator
    {
        [MenuItem("Tests Generators/QuaternionPack")]
        public static void CreateAll()
        {
            var fromTemplate = new CreateFromTemplate("./Assets/Tests/Generators/.QuaternionPackTestTemplate.cs");
            Create(fromTemplate, 8, Quaternion.Euler(0, 90, 0), 0.00540f);
            Create(fromTemplate, 9, Quaternion.Euler(0, 90, 0), 0.00270f);
            Create(fromTemplate, 10, Quaternion.Euler(0, 90, 0), 0.00135f);

            Create(fromTemplate, 8, Quaternion.Euler(45, 90, 0), 0.00540f);
            Create(fromTemplate, 9, Quaternion.Euler(45, 90, 0), 0.00270f);
            Create(fromTemplate, 10, Quaternion.Euler(45, 90, 0), 0.00135f);

            AssetDatabase.Refresh();
        }

        private static void Create(CreateFromTemplate fromTemplate, int bitCount, Quaternion value, float within)
        {
            string name = $"_{bitCount}_{(int)(value.eulerAngles.x)}";
            fromTemplate.Replace("%%NAME%%", name);
            fromTemplate.Replace("%%PACKER_ATTRIBUTE%%", $"{bitCount}");
            fromTemplate.Replace("%%VALUE%%", $"new Quaternion({value.x}f, {value.y}f, {value.z}f, {value.w}f)");
            fromTemplate.Replace("%%WITHIN%%", $"{within}f");
            fromTemplate.Replace("%%BIT_COUNT%%", bitCount * 3 + 2);

            fromTemplate.WriteToFile($"./Assets/Tests/Generated/QuaternionPackTests/QuaternionPackBehaviour{name}.cs");
        }
    }
}
