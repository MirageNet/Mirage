using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mirage.Logging;
using UnityEditor;
using UnityEngine;

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

        Dictionary<string, bool> folderOutState = new Dictionary<string, bool>();

        public override void OnGUI(string searchContext)
        {
            if (settings == null)
            {
                settings = SettingsLoader.Load();
            }

            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            LogType allLogType = GetAllLogLevel();



            using (new GUIScope())
            {
                EditorGUILayout.HelpBox("You may need to run your game a few times for this list to properly populate!", MessageType.Info);
                using (var scope = new EditorGUI.ChangeCheckScope())
                {
                    allLogType = (LogType)EditorGUILayout.EnumPopup("Set All", allLogType);
                    if (scope.changed)
                    {
                        SetAllLogLevels(allLogType);
                    }
                }

                EditorGUILayout.Space();

                foreach (IGrouping<string, LogSettings.LoggerType> group in settings.logLevels.GroupBy(x => x.Namespace))
                {
                    if (!folderOutState.ContainsKey(group.Key))
                        folderOutState[group.Key] = false;

                    folderOutState[group.Key] = EditorGUILayout.Foldout(folderOutState[group.Key], group.Key, EditorStyles.foldoutHeader);

                    if (folderOutState[group.Key])
                    {
                        EditorGUI.indentLevel++;
                        foreach (LogSettings.LoggerType loggerType in group)
                        {
                            using (var scope = new EditorGUI.ChangeCheckScope())
                            {
                                var level = (LogType)EditorGUILayout.EnumPopup(loggerType.name, loggerType.level);

                                if (scope.changed)
                                {
                                    loggerType.level = level;
                                    ILogger logger = LogFactory.GetLogger(loggerType.FullName);
                                    logger.filterLogType = level;
                                    SettingsLoader.EditorSave();
                                }
                            }
                        }
                        EditorGUI.indentLevel--;
                    }

                    EditorGUILayout.Space();
                }

                EditorGUILayout.Space();
                if (GUILayout.Button("Delete All"))
                {
                    settings.logLevels.Clear();
                    SettingsLoader.EditorSave();
                }
                if (GUILayout.Button("Find All type using logger"))
                {
                    Debug.Log(LogFactory.loggers.Count);
                    BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;

                    System.Reflection.Assembly[] loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
                    foreach (System.Reflection.Assembly asm in loadedAssemblies)
                    {
                        foreach (Type type in asm.GetTypes())
                        {
                            foreach (System.Reflection.FieldInfo field in type.GetFields(flags))
                            {
                                if (field.FieldType == typeof(ILogger))
                                {
                                    if (field.IsStatic)
                                    {
                                        var value = (ILogger)field.GetValue(null);
                                        Debug.Log($"{type} {value.filterLogType}");
                                    }
                                }
                            }
                        }
                    }
                    SettingsLoader.AddLogLevelsFromFactory();

                    Debug.Log(LogFactory.loggers.Count);
                }
            }
        }


        private void SetAllLogLevels(LogType allLogType)
        {
            foreach (LogSettings.LoggerType loggerType in settings.logLevels)
            {
                loggerType.level = allLogType;
                ILogger logger = LogFactory.GetLogger(loggerType.FullName);
                logger.filterLogType = allLogType;
                SettingsLoader.EditorSave();
            }
        }

        private LogType GetAllLogLevel()
        {
            List<LogSettings.LoggerType> levels = settings.logLevels;
            if (levels.Count == 0) { return LogType.Warning; }

            bool allSame = true;
            LogType firstLevel = levels[0].level;
            foreach (LogSettings.LoggerType level in levels)
            {
                if (level.level != firstLevel)
                {
                    allSame = false;
                    break;
                }
            }


            if (allSame)
            {
                return firstLevel;
            }
            else
            {
                // -1 => no type, will show as empty dropdown
                return (LogType)(-1);
            }
        }

        private class GUIScope : GUI.Scope
        {
            private readonly float labelWidth;

            public GUIScope()
            {
                GUILayout.BeginVertical();

                labelWidth = EditorGUIUtility.labelWidth;
                if (EditorGUIUtility.currentViewWidth > 550)
                {
                    EditorGUIUtility.labelWidth = 250;
                }
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
