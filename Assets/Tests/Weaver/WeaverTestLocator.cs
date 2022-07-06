using System.IO;
using UnityEditor;
using UnityEngine;

namespace Mirage.Tests.Weaver
{
    /// <summary>
    /// This class is used by weaver tests to find the current directory used for weaver outputs
    /// </summary>
    public class WeaverTestLocator : ScriptableObject
    {
        static string _sourceDirectory;
        public static string SourceDirectory
        {
            get
            {
                if (string.IsNullOrEmpty(_sourceDirectory))
                {
                    ScriptableObject assemblerObj = CreateInstance<WeaverTestLocator>();

                    var monoScript = MonoScript.FromScriptableObject(assemblerObj);
                    string myPath = AssetDatabase.GetAssetPath(monoScript);
                    _sourceDirectory = Path.GetDirectoryName(myPath);
                }
                return _sourceDirectory;
            }
        }

        public static string GetOutputDirectory()
        {
            string directory = Path.Combine(WeaverTestLocator.SourceDirectory, "temp~");
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            return directory;
        }
    }
}
