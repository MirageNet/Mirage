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
            Create(fromTemplate, "short", 4, "3");
            Create(fromTemplate, "short", 12);
            Create(fromTemplate, "ulong", 5);
            Create(fromTemplate, "ulong", 24);
            Create(fromTemplate, "ulong", 64);
            Create(fromTemplate, "MyEnum", 4, "(MyEnum)3",
    @"[System.Flags, System.Serializable]
    public enum MyEnum
    {
        None = 0,
        HasHealth = 1,
        HasArmor = 2,
        HasGun = 4,
        HasAmmo = 8,
    }");

            Create(fromTemplate, "MyByteEnum", 4, "(MyByteEnum)3",
    @"[System.Flags, System.Serializable]
    public enum MyByteEnum : byte
    {
        None = 0,
        HasHealth = 1,
        HasArmor = 2,
        HasGun = 4,
        HasAmmo = 8,
    }");

            AssetDatabase.Refresh();
        }

        private static void Create(CreateFromTemplate fromTemplate, string type, int bitCount, string extraValue = "20", string extraType = "")
        {
            fromTemplate.Replace("%%BIT_COUNT%%", bitCount);
            fromTemplate.Replace("%%TYPE%%", type);
            fromTemplate.Replace("%%PAYLOAD_SIZE%%", Mathf.CeilToInt(bitCount / 8f));
            fromTemplate.Replace("%%EXTRA_TYPE%%", extraType);
            fromTemplate.Replace("%%EXTRA_VALUE%%", extraValue);

            fromTemplate.WriteToFile($"./Assets/Tests/Generated/BitCountTests/BitCountBehaviour_{type}_{bitCount}.cs");
        }
    }
}
