using System.Collections.Generic;
using Mirage.EditorScripts.Logging;
using Mirage.Logging;
using UnityEditor;

namespace Mirage.Settings
{
    public class MirageProjectSettingsProvider : SettingsProvider
    {
        private LogSettings settings;

        public MirageProjectSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords) { }

        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            return new MirageProjectSettingsProvider("Mirage/Logging", SettingsScope.Project) { label = "Logging" };
        }

        private LogLevelsGUI _drawer;
        private LogLevelsGUI GetDrawer(LogSettings settings)
        {
            if (_drawer == null)
            {
                _drawer = new LogLevelsGUI(settings);
            }
            return _drawer;
        }

        public override void OnGUI(string searchContext)
        {
            if (settings == null)
            {
                settings = EditorLogSettingsLoader.FindLogSettings();
                if (settings == null)
                    settings = LogLevelsGUI.DrawCreateNewButton();
            }
            else
            {
                LogLevelsGUI drawer = GetDrawer(settings);
                drawer.Draw2();
            }
        }
    }
}
