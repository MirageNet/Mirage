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
            var fromTemplate = new CreateFromTemplate("./Assets/Tests/Generators/.VarIntTestTemplate.cs");
            Create(fromTemplate, "int", 100, 10000, null, new[] { "10", "100", "1000" }, new[] { 7 + 1, 7 + 1, 14 + 2 });
            Create(fromTemplate, "int", 100, 1000, 10000, new[] { "10", "100", "1000", "10000" }, new[] { 7 + 1, 7 + 1, 10 + 2, 14 + 2 });
            Create(fromTemplate, "uint", 100, 1000, 10000, new[] { "10U", "100U", "1000U", "10000U" }, new[] { 7 + 1, 7 + 1, 10 + 2, 14 + 2 });
            Create(fromTemplate, "uint", 255, 64000, null, new[] { "170U", "500U", "15000U", "50000U" }, new[] { 8 + 1, 16 + 2, 16 + 2, 16 + 2 });
            Create(fromTemplate, "uint", 500, 32000, 2_000_000, new[] { "170U", "500U", "15000U", "50000U", "400000U" }, new[] { 9 + 1, 9 + 1, 15 + 2, 21 + 2, 21 + 2 });

            Create(fromTemplate, "short", 100, 1000, 10000, new[] { "(short)10", "(short)100", "(short)1000", "(short)10000" }, new[] { 7 + 1, 7 + 1, 10 + 2, 14 + 2 });
            Create(fromTemplate, "ushort", 100, 1000, 10000, new[] { "(ushort)10", "(ushort)100", "(ushort)1000", "(ushort)10000" }, new[] { 7 + 1, 7 + 1, 10 + 2, 14 + 2 });

            Create(fromTemplate, "long", 100, 1000, 10000, new[] { "10L", "100L", "1000L", "10000L" }, new[] { 7 + 1, 7 + 1, 10 + 2, 14 + 2 });
            Create(fromTemplate, "ulong", 100, 1000, 10000, new[] { "10UL", "100UL", "1000UL", "10000UL" }, new[] { 7 + 1, 7 + 1, 10 + 2, 14 + 2 });
            Create(fromTemplate, "MyEnum", 4, 64, null, new[] { "(MyEnum)0", "(MyEnum)4", "(MyEnum)16", "(MyEnum)64" }, new[] { 3 + 1, 3 + 1, 7 + 2, 7 + 2 },
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

            Create(fromTemplate, "MyEnumByte", 4, 64, null, new[] { "(MyEnumByte)0", "(MyEnumByte)4", "(MyEnumByte)16", "(MyEnumByte)64" }, new[] { 3 + 1, 3 + 1, 7 + 2, 7 + 2 },
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

        private static void Create(CreateFromTemplate fromTemplate, string type, int smallMax, int mediumMax, int? largeMax, string[] values, int[] expectedBitCount, string extraType = "")
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

            string name = $"{type}_{smallMax}_{mediumMax}";
            fromTemplate.Replace("%%NAME%%", name);

            fromTemplate.WriteToFile($"./Assets/Tests/Generated/VarIntTests/VarIntBehaviour_{name}.cs");
        }
    }
}
