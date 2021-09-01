using System.Text;
using JamesFrowen.SimpleCodeGen;
using UnityEditor;

namespace Mirage.Tests.CodeGenerators
{
    public static class VarIntBlocksTestGenerator
    {
        [MenuItem("Tests Generators/VarIntBlocks")]
        public static void CreateAll()
        {
            var fromTemplate = new CreateFromTemplate("./Assets/Tests/Generators/.VarIntBlocksTestTemplate.txt");
            Create(fromTemplate, "int", 7, new[] { "10", "100", "1000" }, new[] { 7 + 1, 7 + 1, 14 + 2 });
            Create(fromTemplate, "int", 6, new[] { "10", "100", "1000", "10000" }, new[] { 6 + 1, 12 + 2, 12 + 2, 18 + 3 });
            Create(fromTemplate, "uint", 6, new[] { "170", "500", "15000", "50000" }, new[] { 12 + 2, 12 + 2, 18 + 3, 18 + 3 });
            Create(fromTemplate, "uint", 7, new[] { "10", "100", "1000", "10000" }, new[] { 7 + 1, 7 + 1, 14 + 2, 14 + 2 });
            Create(fromTemplate, "uint", 8, new[] { "170", "500", "15000", "50000", "400000" }, new[] { 8 + 1, 16 + 2, 16 + 2, 16 + 2, 24 + 3 });

            Create(fromTemplate, "short", 6, new[] { "10", "100", "1000", "10000" }, new[] { 6 + 1, 12 + 2, 12 + 2, 18 + 3 });
            Create(fromTemplate, "ushort", 7, new[] { "10", "100", "1000", "10000" }, new[] { 7 + 1, 7 + 1, 14 + 2, 14 + 2 });

            Create(fromTemplate, "long", 8, new[] { "10", "100", "1000", "10000" }, new[] { 8 + 1, 8 + 1, 16 + 2, 16 + 2 });
            Create(fromTemplate, "ulong", 9, new[] { "10", "100", "1000", "10000" }, new[] { 9 + 1, 9 + 1, 18 + 2, 18 + 2 });
            Create(fromTemplate, "MyEnum", 4, new[] { "(MyEnum)0", "(MyEnum)4", "(MyEnum)16", "(MyEnum)64" }, new[] { 4 + 1, 4 + 1, 8 + 2, 8 + 2 },
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

            Create(fromTemplate, "MyEnumByte", 4, new[] { "(MyEnumByte)0", "(MyEnumByte)4", "(MyEnumByte)16", "(MyEnumByte)64" }, new[] { 4 + 1, 4 + 1, 8 + 2, 8 + 2 },
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

        private static void Create(CreateFromTemplate fromTemplate, string type, int bitCount, string[] values, int[] expectedBitCount, string extraType = "")
        {
            fromTemplate.Replace("%%TYPE%%", type);
            fromTemplate.Replace("%%BLOCK_SIZE%%", bitCount);
            fromTemplate.Replace("%%EXTRA_TYPE%%", extraType);

            var testCase = new StringBuilder();
            for (int i = 0; i < values.Length; i++)
            {
                testCase.AppendLine($"        [TestCase({values[i]}, {expectedBitCount[i]})]");
            }
            fromTemplate.Replace($"%%TEST_CASES%%", testCase.ToString());

            string name = $"{type}_{bitCount}";
            fromTemplate.Replace("%%NAME%%", name);

            fromTemplate.WriteToFile($"./Assets/Tests/Generated/VarIntBlocksTests/VarIntBlocksBehaviour_{name}.cs");
        }
    }
}
