using Mirage.Logging;
using UnityEditor;

namespace Mirage.EditorScripts.Logging
{
    [CustomEditor(typeof(NetworkLogSettings))]
    public class NetworkLogSettingsEditor : Editor
    {
        private LogLevelsGUI _drawer;

        private LogLevelsGUI GetDrawer(LogSettings settings)
        {
            if (_drawer == null)
            {
                _drawer = new LogLevelsGUI(settings);
            }
            return _drawer;
        }
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var target = this.target as NetworkLogSettings;

            if (target.settings == null)
            {
                LogSettings newSettings = LogLevelsGUI.DrawCreateNewButton();
                if (newSettings != null)
                {
                    SerializedProperty settingsProp = serializedObject.FindProperty("settings");
                    settingsProp.objectReferenceValue = newSettings;
                    serializedObject.ApplyModifiedProperties();
                }
            }
            else
            {
                GetDrawer(target.settings).Draw();
            }
        }
    }
}
