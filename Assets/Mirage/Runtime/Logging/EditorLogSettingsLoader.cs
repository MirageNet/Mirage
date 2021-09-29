using UnityEditor;
using UnityEngine;

namespace Mirage.Logging
{
#if UNITY_EDITOR
    public static class EditorLogSettingsLoader
    {
        static LogSettingsSO cache;

        [InitializeOnLoadMethod]
        static void Init()
        {
            // load settings first time LogFactory is used in the editor
            LoadLogSettingsIntoDictionary();
        }

        public static void LoadLogSettingsIntoDictionary()
        {
            LogSettingsSO settings = FindLogSettings();
            if (settings != null)
            {
                settings.LoadIntoLogFactory();
            }
        }

        public static LogSettingsSO FindLogSettings()
        {
            if (cache != null)
                return cache;

            cache = Object.FindObjectOfType<LogSettingsSO>();
            if (cache != null)
                return cache;

            string[] assetGuids = AssetDatabase.FindAssets("t:" + nameof(LogSettingsSO));
            if (assetGuids.Length == 0)
                return null;

            string firstGuid = assetGuids[0];

            string path = AssetDatabase.GUIDToAssetPath(firstGuid);
            cache = AssetDatabase.LoadAssetAtPath<LogSettingsSO>(path);

            if (assetGuids.Length > 2)
            {
                Debug.LogWarning("Found more than one LogSettings, Delete extra settings. Using first asset found: " + path);
            }
            Debug.Assert(cache != null, "Failed to load asset at: " + path);

            return cache;
        }
    }
#endif
}
