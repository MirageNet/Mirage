using System.IO;
using UnityEditor;
using UnityEngine;

namespace Mirage.Weaver
{
    /// <summary>
    /// This class is used by weaver tests to find the current directory used for weaver outputs
    /// </summary>
    public class WeaverTestLocator : ScriptableObject
    {
        static string _outputDirectory;
        public static string OutputDirectory
        {
            get
            {
                if (string.IsNullOrEmpty(_outputDirectory))
                {
                    ScriptableObject assemblerObj = CreateInstance<WeaverTestLocator>();

                    var monoScript = MonoScript.FromScriptableObject(assemblerObj);
                    string myPath = AssetDatabase.GetAssetPath(monoScript);
                    _outputDirectory = Path.GetDirectoryName(myPath);
                }
                return _outputDirectory;
            }
        }
    }
}
