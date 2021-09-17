using UnityEditor;

namespace Mirage.Tests.CodeGenerators
{
    public static class TestGenerators
    {
        [MenuItem("Tests Generators/CreateAll", priority = 100)]
        public static void CreateAll()
        {
            BitCountFromRangeTestGenerator.CreateAll();
            BitCountTestGenerator.CreateAll();
            FloatPackTestGenerator.CreateAll();
            QuaternionPackTestGenerator.CreateAll();
            VarIntBlocksTestGenerator.CreateAll();
            VarIntTestGenerator.CreateAll();
            Vector2PackTestGenerator.CreateAll();
            Vector3PackTestGenerator.CreateAll();
            ZigZagTestGenerator.CreateAll();
        }
    }
}
