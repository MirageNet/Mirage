using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Mirage
{
    public class MirageProjectSettingsProvider : SettingsProvider
    {
        private MirageProjectSettings settings;

        public MirageProjectSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords) { }

        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            return new MirageProjectSettingsProvider("Mirage/Logging", SettingsScope.Project) { label = "Logging" };
        }

        public override void OnGUI(string searchContext)
        {
            if (settings == null)
            {
                settings = MirageProjectSettings.Get();
            }

            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            LogType allLogType = LogType.Warning;

            bool same = true;
            for (int i = 0; i < settings.logLevels.Count; i++)
            {
                if (i == 0)
                {
                    allLogType = settings.logLevels[i].level;
                }

                for (int j = i; j < settings.logLevels.Count; j++)
                {
                    if (settings.logLevels[i].level != settings.logLevels[j].level)
                    {
                        same = false;
                        break;
                    }
                }

                if (!same)
                {
                    break;
                }
            }

            if (!same)
            {
                allLogType = LogType.Warning;
            }

            using (new GUIScope())
            {
                EditorGUILayout.HelpBox("You may need to run your game a few times for this list to properly populate!", MessageType.Info);
                allLogType = (LogType)EditorGUILayout.EnumPopup("Set All", allLogType);
                if (EditorGUI.EndChangeCheck())
                {
                    for (int i = 0; i < settings.logLevels.Count; i++)
                    {
                        MirageProjectSettings.Level levelInfo = settings.logLevels[i];
                        levelInfo.level = allLogType;
                        settings.logLevels[i] = levelInfo;
                        settings.EditorSave();
                    }
                }

                EditorGUILayout.Space();

                for (int i = 0; i < settings.logLevels.Count; i++)
                {
                    EditorGUI.BeginChangeCheck();
                    LogType level = settings.logLevels[i].level;
                    level = (LogType)EditorGUILayout.EnumPopup(settings.logLevels[i].name, level);
                    if (EditorGUI.EndChangeCheck())
                    {
                        MirageProjectSettings.Level levelInfo = settings.logLevels[i];
                        levelInfo.level = level;
                        settings.logLevels[i] = levelInfo;
                        settings.EditorSave();
                    }
                }
            }
        }

        private class GUIScope : GUI.Scope
        {
            private readonly float labelWidth;

            public GUIScope()
            {
                GUILayout.BeginVertical();

                labelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 250;
                GUILayout.BeginHorizontal();
                GUILayout.Space(7);
                GUILayout.BeginVertical();
                GUILayout.Space(4);
            }

            protected override void CloseScope()
            {
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                EditorGUIUtility.labelWidth = labelWidth;
            }
        }
    }
}
