using UnityEngine;

namespace Mirage.Logging
{
    /// <summary>
    /// Used to load LogSettings in build
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Network/NetworkLogSettings")]
    [HelpURL("https://mirror-networking.com/docs/Articles/Components/NetworkLogSettings.html")]
    public class LogSettings : MonoBehaviour
    {
        [Header("Log Settings Asset")]
        [SerializeField] internal LogSettingsSO settings;

#if UNITY_EDITOR
        // called when component is added to GameObject
        void Reset()
        {
            LogSettingsSO existingSettings = EditorLogSettingsLoader.FindLogSettings();
            if (existingSettings != null)
            {
                settings = existingSettings;

                UnityEditor.EditorUtility.SetDirty(this);
            }
        }
#endif

        void Awake()
        {
            RefreshDictionary();
        }

        void OnValidate()
        {
            // if settings field is changed
            RefreshDictionary();
        }

        void RefreshDictionary()
        {
            settings.LoadIntoDictionary(LogFactory.loggers);
        }
    }
}
