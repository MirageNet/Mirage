using System.Collections.Generic;
using Mirage.Logging;
using UnityEditor;

namespace Mirage.EditorScripts.Logging
{
    public class LogSettingsProvider : SettingsProvider
    {
        private LogSettingsSO settings;

        public LogSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords) { }

        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            return new LogSettingsProvider("Mirage/Logging", SettingsScope.Project) { label = "Logging" };
        }

        public override void OnGUI(string searchContext)
        {
            // look for existing settings first
            if (settings == null)
            {
                settings = EditorLogSettingsLoader.FindLogSettings();
            }

            // then draw field
            settings = (LogSettingsSO)EditorGUILayout.ObjectField("Settings", settings, typeof(LogSettingsSO), false);

            // then draw rest of ui
            if (settings == null)
                settings = LogLevelsGUI.DrawCreateNewButton();
            else
                LogLevelsGUI.DrawSettings(settings);
        }
    }
}
