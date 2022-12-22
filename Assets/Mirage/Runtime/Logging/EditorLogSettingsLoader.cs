using UnityEditor;
using UnityEngine;

namespace Mirage.Logging
{
#if UNITY_EDITOR
    public static class EditorLogSettingsLoader
    {
        private static LogSettingsSO cache;

        [InitializeOnLoadMethod]
        private static void Init()
        {
            // load settings first time LogFactory is used in the editor
            LoadLogSettingsIntoDictionary();
        }

        public static void LoadLogSettingsIntoDictionary()
        {
            var settings = FindLogSettings();
            if (settings != null)
            {
                settings.LoadIntoLogFactory();
            }
        }

        public static LogSettingsSO FindLogSettings()
        {
            if (cache != null)
                return cache;

#if UNITY_2023_1_OR_NEWER
            cache = Object.FindFirstObjectByType<LogSettingsSO>();
#else
            cache = Object.FindObjectOfType<LogSettingsSO>();
#endif
            if (cache != null)
                return cache;

            var assetGuids = AssetDatabase.FindAssets("t:" + nameof(LogSettingsSO));
            if (assetGuids.Length == 0)
                return null;

            var firstGuid = assetGuids[0];

            var path = AssetDatabase.GUIDToAssetPath(firstGuid);
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
