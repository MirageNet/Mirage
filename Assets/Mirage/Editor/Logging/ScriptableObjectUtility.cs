using UnityEditor;
using UnityEngine;

namespace Mirage.EditorScripts.Logging
{
    internal static class ScriptableObjectUtility
    {
        /// <summary>
        ///	This makes it easy to create, name and place unique new ScriptableObject asset files.
        /// </summary>
        public static T CreateAsset<T>(string defaultName, string defaultPath) where T : ScriptableObject
        {
            var path = SavePanel(defaultName, defaultPath);
            // user click cancel
            if (string.IsNullOrEmpty(path)) { return null; }

            var asset = ScriptableObject.CreateInstance<T>();

            SaveAsset(path, asset);

            return asset;
        }

        private static string SavePanel(string name, string defaultPath)
        {
            var path = EditorUtility.SaveFilePanel(
                           "Save ScriptableObject",
                           defaultPath,
                           name + ".asset",
                           "asset");

            // user click cancel, return early
            if (string.IsNullOrEmpty(path)) { return path; }

            // Unity only wants path from Assets
            if (path.StartsWith(Application.dataPath))
            {
                path = "Assets" + path.Substring(Application.dataPath.Length);
            }

            return path;
        }

        private static void SaveAsset(string path, ScriptableObject asset)
        {
            var assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path);

            AssetDatabase.CreateAsset(asset, assetPathAndName);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
