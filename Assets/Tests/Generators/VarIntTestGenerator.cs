using System.Text;
using JamesFrowen.SimpleCodeGen;
using UnityEditor;

namespace Mirage.Tests.CodeGenerators
{
    public static class VarIntTestGenerator
    {
        [MenuItem("Tests Generators/VarInt")]
        public static void CreateAll()
        {
            var fromTemplate = new CreateFromTemplate("./Assets/Tests/Generators/.VarIntTestTemplate.txt");
            Create(fromTemplate, "int", 100, 10000, null, new[] { "10", "100", "1000" }, new[] { 7 + 1, 7 + 1, 14 + 1 });
            Create(fromTemplate, "int", 100, 1000, 10000, new[] { "10", "100", "1000", "10000" }, new[] { 7 + 1, 7 + 1, 10 + 2, 14 + 2 });
            Create(fromTemplate, "uint", 100, 1000, 10000, new[] { "10", "100", "1000", "10000" }, new[] { 7 + 1, 7 + 1, 10 + 2, 14 + 2 });
            Create(fromTemplate, "uint", 256, 64000, null, new[] { "170", "500", "15000", "50000" }, new[] { 8 + 1, 16 + 1, 16 + 1, 16 + 1 });
            Create(fromTemplate, "uint", 500, 32000, 2_000_000, new[] { "170", "500", "15000", "50000", "400000" }, new[] { 9 + 1, 9 + 1, 15 + 2, 21 + 2, 21 + 2 });

            Create(fromTemplate, "short", 100, 1000, 10000, new[] { "10", "100", "1000", "10000" }, new[] { 7 + 1, 7 + 1, 10 + 2, 14 + 2 });
            Create(fromTemplate, "ushort", 100, 1000, 10000, new[] { "10", "100", "1000", "10000" }, new[] { 7 + 1, 7 + 1, 10 + 2, 14 + 2 });

            Create(fromTemplate, "long", 100, 1000, 10000, new[] { "10", "100", "1000", "10000" }, new[] { 7 + 1, 7 + 1, 10 + 2, 14 + 2 });
            Create(fromTemplate, "ulong", 100, 1000, 10000, new[] { "10", "100", "1000", "10000" }, new[] { 7 + 1, 7 + 1, 10 + 2, 14 + 2 });
            Create(fromTemplate, "MyEnum", 4, 64, null, new[] { "(MyEnum)0", "(MyEnum)4", "(MyEnum)16", "(MyEnum)64" }, new[] { 4 + 1, 4 + 1, 6 + 1, 6 + 1 },
    @"[System.Flags, System.Serializable]
    public enum MyEnum
    {
        None = 0,
        HasHealth = 1,
        HasArmor = 2,
        HasGun = 4,
        HasAmmo = 8,
        HasLeftHand = 16,
        HasRightHand = 32,
        HasHead = 64,
    }");

            Create(fromTemplate, "MyEnumByte", 4, 64, null, new[] { "(MyEnum)0", "(MyEnum)4", "(MyEnum)16", "(MyEnum)64" }, new[] { 4 + 1, 4 + 1, 6 + 1, 6 + 1 },
    @"[System.Flags, System.Serializable]
    public enum MyEnumByte : byte
    {
        None = 0,
        HasHealth = 1,
        HasArmor = 2,
        HasGun = 4,
        HasAmmo = 8,
        HasLeftHand = 16,
        HasRightHand = 32,
        HasHead = 64,
    }");

            AssetDatabase.Refresh();
        }

        private static void Create(CreateFromTemplate fromTemplate, string type, int smallMax, int mediumMax, int? largeMax, string[] values, int[] expectedBitCount, string extraType = "", string extraName = "")
        {
            fromTemplate.Replace("%%TYPE%%", type);


            string args = largeMax.HasValue
                ? $"{smallMax}, {mediumMax}, {largeMax.Value}"
                : $"{smallMax}, {mediumMax}";
            fromTemplate.Replace("%%ARGS%%", args);
            fromTemplate.Replace("%%EXTRA_TYPE%%", extraType);

            var testCase = new StringBuilder();
            for (int i = 0; i < values.Length; i++)
            {
                testCase.AppendLine($"        [TestCase({values[i]}, {expectedBitCount[i]})]");
            }
            fromTemplate.Replace($"%%TEST_CASES%%", testCase.ToString());

            string name = $"{type}_{smallMax}_{mediumMax}{extraName}";
            fromTemplate.Replace("%%NAME%%", name);

            fromTemplate.WriteToFile($"./Assets/Tests/Generated/VarIntTests/VarIntBehaviour_{name}.cs");
        }
    }
}
