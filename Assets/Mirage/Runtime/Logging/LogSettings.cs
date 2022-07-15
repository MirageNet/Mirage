using UnityEditor;
using UnityEngine;

namespace Mirage.Logging
{
    /// <summary>
    /// Used to load LogSettings in build
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Network/LogSettings")]
    [HelpURL("https://miragenet.github.io/Mirage/Articles/Components/NetworkLogSettings.html")]
    public class LogSettings : MonoBehaviour
    {
        [Header("Log Settings Asset")]
        [SerializeField] internal LogSettingsSO settings;

#if UNITY_EDITOR
        // called when component is added to GameObject
        private void Reset()
        {
            if (settings != null) { return; }

            LogSettingsSO existingSettings = EditorLogSettingsLoader.FindLogSettings();
            if (existingSettings != null)
            {
                Undo.RecordObject(this, "adding existing settings");
                settings = existingSettings;
            }
        }
#endif

        private void Awake()
        {
            RefreshDictionary();
        }

        private void OnValidate()
        {
            RefreshDictionary();
        }

        private void RefreshDictionary()
        {
            if (settings != null)
            {
                settings.LoadIntoLogFactory();
            }
            else
            {
                Debug.LogWarning("Log settings component does not have a settings reference", this);
            }
        }
    }
}
