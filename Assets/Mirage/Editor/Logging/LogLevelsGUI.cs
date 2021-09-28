using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JamesFrowen.EditorScripts;
using Mirage.Logging;
using UnityEditor;
using UnityEngine;

namespace Mirage.EditorScripts.Logging
{
    public class LogLevelsGUI
    {
        private static LogLevelsGUI _drawer;

        public static void DrawStatic(LogSettings settings)
        {
            if (_drawer == null)
            {
                _drawer = new LogLevelsGUI(settings);
            }

            Debug.Assert(_drawer.settings == settings);
            _drawer.Draw2();
        }

        public static LogSettings DrawCreateNewButton()
        {
            if (GUILayout.Button("Create New"))
            {
                return ScriptableObjectUtility.CreateAsset<LogSettings>(nameof(LogSettings));
            }

            return null;
        }

        readonly LogSettings settings;
        readonly Dictionary<string, bool> folderOutState = new Dictionary<string, bool>();

        /// <summary>
        /// Keep track of gui changed. If it has changed then we need to update <see cref="LogFactory"/> and save the new levels to file
        /// </summary>
        bool guiChanged;

        public LogLevelsGUI(LogSettings settings)
        {
            this.settings = settings;
        }

        public void Draw2()
        {
            guiChanged = false;
            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();

            using (new LogGUIScope())
            {
                EditorGUILayout.HelpBox("You may need to run your game a few times for this list to properly populate!", MessageType.Info);
                DrawAllLevelDropdown();

                EditorGUILayout.Space();

                foreach (IGrouping<string, LogSettings.LoggerSettings> group in settings.LogLevels.GroupBy(x => x.Namespace))
                {
                    DrawGroup(group);

                    EditorGUILayout.Space();
                }

                EditorGUILayout.Space();
                DrawDeleteAllButton();
                DrawFindAllButton();
            }

            if (guiChanged)
            {
                ApplyAndSaveLevels();
            }
        }

        private void DrawAllLevelDropdown()
        {
            using (var scope = new EditorGUI.ChangeCheckScope())
            {
                LogType allLogType = GetGroupLevel(settings.LogLevels);
                allLogType = (LogType)EditorGUILayout.EnumPopup("Set All", allLogType);
                if (scope.changed)
                {
                    SetGroupLevel(settings.LogLevels, allLogType);
                }
            }
        }

        private void DrawGroup(IGrouping<string, LogSettings.LoggerSettings> group)
        {
            string NameSpace = group.Key;
            if (!folderOutState.ContainsKey(NameSpace))
                folderOutState[NameSpace] = false;

            folderOutState[NameSpace] = EditorGUILayout.Foldout(folderOutState[NameSpace], NameSpace, EditorStyles.foldoutHeader);

            if (folderOutState[NameSpace])
            {
                EditorGUI.indentLevel++;
                foreach (LogSettings.LoggerSettings loggerType in group)
                {
                    using (var scope = new EditorGUI.ChangeCheckScope())
                    {
                        var level = (LogType)EditorGUILayout.EnumPopup(loggerType.Name, loggerType.logLevel);

                        if (scope.changed)
                        {
                            loggerType.logLevel = level;
                            guiChanged = true;
                        }
                    }
                }
                EditorGUI.indentLevel--;
            }
        }

        private void DrawDeleteAllButton()
        {
            if (GUILayout.Button("Clear All levels"))
            {
                settings.LogLevels.Clear();
                guiChanged = true;
            }
        }

        private void DrawFindAllButton()
        {
            if (GUILayout.Button("Find All type using logger"))
            {
                // todo remove logs
                Debug.Log(LogFactory.loggers.Count);

                BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;

                Assembly[] loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (Assembly asm in loadedAssemblies)
                {
                    foreach (Type type in asm.GetTypes())
                    {
                        foreach (FieldInfo field in type.GetFields(flags))
                        {
                            if (field.IsStatic && field.FieldType == typeof(ILogger))
                            {
                                var value = (ILogger)field.GetValue(null);
                                Debug.Log($"{type} {value.filterLogType}");
                            }
                        }
                    }
                }
                guiChanged = true;
                Debug.Log(LogFactory.loggers.Count);
            }
        }

        private void ApplyAndSaveLevels()
        {
            foreach (LogSettings.LoggerSettings logSetting in settings.LogLevels)
            {
                ILogger logger = LogFactory.GetLogger(logSetting.FullName);
                logger.filterLogType = logSetting.logLevel;
            }

            // tood save outside of editor
            EditorUtility.SetDirty(settings);
        }

        private LogType GetGroupLevel(IEnumerable<LogSettings.LoggerSettings> group)
        {
            if (!group.Any()) { return LogType.Warning; }

            IEnumerable<LogType> distinctLevels = group.Select(x => x.logLevel).Distinct();
            bool allSame = distinctLevels.Count() == 1;

            if (allSame)
            {
                return distinctLevels.First();
            }
            else
            {
                // -1 => no type, will show as empty dropdown
                return (LogType)(-1);
            }
        }
        private void SetGroupLevel(IEnumerable<LogSettings.LoggerSettings> group, LogType level)
        {
            foreach (LogSettings.LoggerSettings logger in group)
            {
                logger.logLevel = level;
            }
        }

        private class LogGUIScope : GUI.Scope
        {
            private readonly float labelWidth;

            public LogGUIScope()
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

        [System.Obsolete("old draw", true)]
        public void Draw()
        {
            using (var scope = new EditorGUI.ChangeCheckScope())
            {
                if (LogFactory.loggers.Count == 0)
                {
                    EditorGUILayout.LabelField("No Keys found in LogFactory.loggers\nPlay the game for default log values to be added to LogFactory", EditorStyles.wordWrappedLabel);
                }
                else
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Logging Components", EditorStyles.boldLabel);

                    foreach (KeyValuePair<string, ILogger> item in LogFactory.loggers)
                    {
                        DrawLoggerField(item);
                    }

                    if (scope.changed)
                    {
                        settings.SaveFromDictionary(LogFactory.loggers);
                    }
                }
            }
        }

        static void DrawLoggerField(KeyValuePair<string, ILogger> item)
        {
            ILogger logger = item.Value;
            string name = item.Key;

            const float fieldWidth = 100f;
            const float inspectorMargin = 25f;

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(new GUIContent(ObjectNames.NicifyVariableName(name)), GUILayout.MaxWidth(EditorGUIUtility.currentViewWidth - fieldWidth - inspectorMargin));
                logger.filterLogType = (LogType)EditorGUILayout.EnumPopup(logger.filterLogType, GUILayout.Width(fieldWidth));
            }
        }
    }
}

namespace JamesFrowen.EditorScripts
{
    public static class ScriptableObjectUtility
    {
        /// <summary>
        //	This makes it easy to create, name and place unique new ScriptableObject asset files.
        /// </summary>
        public static T CreateAsset<T>(string defaultName) where T : ScriptableObject
        {
            string path = SavePanel(defaultName);
            // user click cancel
            if (string.IsNullOrEmpty(path)) { return null; }

            T asset = ScriptableObject.CreateInstance<T>();

            SaveAsset(path, asset);

            return asset;
        }

        static string SavePanel(string name)
        {
            string path = EditorUtility.SaveFilePanel(
                           "Save ScriptableObject",
                           "Assets/Mirror/",
                           name + ".asset",
                           "asset");

            // user click cancel, return early
            if (string.IsNullOrEmpty(path)) { return path; }

            // Unity only wants path from Assets
            if (path.StartsWith(Application.dataPath))
            {
                path = "Assets" + path.Substring(Application.dataPath.Length);
            }

            return path;
        }

        static void SaveAsset(string path, ScriptableObject asset)
        {
            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path);

            AssetDatabase.CreateAsset(asset, assetPathAndName);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
