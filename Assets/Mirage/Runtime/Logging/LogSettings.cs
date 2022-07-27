using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Mirage.Logging
{
    /// <summary>
    /// Used to load LogSettings in build
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Network/LogSettings")]
    [HelpURL("https://miragenet.github.io/Mirage/docs/components/network-log-settings")]
    public class LogSettings : MonoBehaviour
    {
        [Header("Log Settings Asset")]
        [FormerlySerializedAs("settings")]
        [SerializeField] internal LogSettingsSO _settings;

#if UNITY_EDITOR
        // called when component is added to GameObject
        private void Reset()
        {
            if (_settings != null) { return; }

            var existingSettings = EditorLogSettingsLoader.FindLogSettings();
            if (existingSettings != null)
            {
                Undo.RecordObject(this, "adding existing settings");
                _settings = existingSettings;
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
            if (_settings != null)
            {
                _settings.LoadIntoLogFactory();
            }
            else
            {
                Debug.LogWarning("Log settings component does not have a settings reference", this);
            }
        }
    }
}
