using Mirage.Logging;
using UnityEditor;

namespace Mirage.EditorScripts.Logging
{
    [CustomEditor(typeof(LogSettings)), CanEditMultipleObjects]
    public class LogSettingsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var target = this.target as LogSettings;

            if (target._settings == null)
            {
                var newSettings = LogLevelsGUI.DrawCreateNewButton();
                if (newSettings != null)
                {
                    var settingsProp = serializedObject.FindProperty("settings");
                    settingsProp.objectReferenceValue = newSettings;
                    serializedObject.ApplyModifiedProperties();
                }
            }
            else
            {
                LogLevelsGUI.DrawSettings(target._settings);
            }
        }
    }
}
