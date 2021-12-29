using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mirage.Logging;
using UnityEditor;
using UnityEngine;

namespace Mirage.EditorScripts.Logging
{
    public class LogLevelsGUI
    {
        private static LogLevelsGUI _drawer;

        public static void DrawSettings(LogSettingsSO settings)
        {
            if (_drawer == null)
            {
                _drawer = new LogLevelsGUI(settings);
            }

            Debug.Assert(_drawer.settings == settings);
            _drawer.Draw();
        }

        public static LogSettingsSO DrawCreateNewButton()
        {
            if (GUILayout.Button("Create New Settings"))
            {
                LogSettingsSO newSettings = ScriptableObjectUtility.CreateAsset<LogSettingsSO>(nameof(LogSettingsSO), "Assets");
                newSettings.SaveFromLogFactory();
                return newSettings;
            }

            return null;
        }

        readonly LogSettingsSO settings;
        readonly LogSettingChecker checker;
        readonly Dictionary<string, bool> folderOutState = new Dictionary<string, bool>();

        /// <summary>
        /// Keep track of gui changed. If it has changed then we need to update <see cref="LogFactory"/> and save the new levels to file
        /// </summary>
        bool guiChanged;

        public LogLevelsGUI(LogSettingsSO settings)
        {
            this.settings = settings;
            checker = new LogSettingChecker(settings);
        }

        public void Draw()
        {
            checker.Refresh();

            guiChanged = false;

            EditorGUI.BeginChangeCheck();

            using (new LogGUIScope())
            {
                EditorGUILayout.HelpBox("You may need to run your game a few times for this list to properly populate!", MessageType.Info);
                DrawAllLevelDropdown();

                EditorGUILayout.Space();

                foreach (IGrouping<string, LogSettingsSO.LoggerSettings> group in settings.LogLevels.GroupBy(x => x.Namespace).OrderBy(x => x.Key))
                {
                    DrawGroup(group);
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

        private void DrawGroup(IGrouping<string, LogSettingsSO.LoggerSettings> group)
        {
            string NameSpace = group.Key ?? "<none>";
            if (!folderOutState.ContainsKey(NameSpace))
                folderOutState[NameSpace] = false;

            folderOutState[NameSpace] = EditorGUILayout.Foldout(folderOutState[NameSpace], NameSpace, toggleOnLabelClick: true, EditorStyles.foldoutHeader);


            if (folderOutState[NameSpace])
            {
                EditorGUI.indentLevel++;
                foreach (LogSettingsSO.LoggerSettings loggerType in group.OrderBy(x => x.Name))
                {
                    using (var scope = new EditorGUI.ChangeCheckScope())
                    {
                        LogType level = DrawNiceEnum(loggerType);

                        if (scope.changed)
                        {
                            loggerType.logLevel = level;
                            guiChanged = true;
                        }
                    }
                }
                EditorGUI.indentLevel--;

                // draw a space after open foldout
                EditorGUILayout.Space();
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
                BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;

                Assembly[] loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (Assembly asm in loadedAssemblies)
                {
                    foreach (Type type in asm.GetTypes())
                    {
                        // skip unity so that we dont fine Debug.Logger
                        if (type.FullName.StartsWith("UnityEngine."))
                            continue;

                        // Can't load fields for generic types
                        if (type.IsGenericType)
                            continue;

                        foreach (FieldInfo field in type.GetFields(flags))
                        {
                            try
                            {
                                if (field.IsStatic && field.FieldType == typeof(ILogger))
                                {
                                    var value = (ILogger)field.GetValue(null);
                                    AddIfMissing(type, value);
                                }
                            }
                            catch (Exception e)
                            {
                                Debug.LogError($"Failed to find Logger inside type {type.Name} with exception:{e}");
                            }
                        }
                    }
                }
                guiChanged = true;
            }
        }

        private void AddIfMissing(Type type, ILogger logger)
        {
            string fullName = type.FullName;
            LogType logType = logger.filterLogType;
            bool exist = settings.LogLevels.Any(x => x.FullName == fullName);
            if (!exist)
            {
                settings.LogLevels.Add(new LogSettingsSO.LoggerSettings(fullName, logType));
            }
        }

        private void ApplyAndSaveLevels()
        {
            foreach (LogSettingsSO.LoggerSettings logSetting in settings.LogLevels)
            {
                ILogger logger = LogFactory.GetLogger(logSetting.FullName);
                logger.filterLogType = logSetting.logLevel;
            }

            // tood save outside of editor
            EditorUtility.SetDirty(settings);
        }

        private LogType GetGroupLevel(IEnumerable<LogSettingsSO.LoggerSettings> group)
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
        private void SetGroupLevel(IEnumerable<LogSettingsSO.LoggerSettings> group, LogType level)
        {
            foreach (LogSettingsSO.LoggerSettings logger in group)
            {
                logger.logLevel = level;
            }
        }

        private static LogType DrawNiceEnum(LogSettingsSO.LoggerSettings loggerType)
        {
            string name = loggerType.Name;
            LogType level = loggerType.logLevel;

            return (LogType)EditorGUILayout.EnumPopup(ObjectNames.NicifyVariableName(name), level);
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
    }

    internal static class ScriptableObjectUtility
    {
        /// <summary>
        ///	This makes it easy to create, name and place unique new ScriptableObject asset files.
        /// </summary>
        public static T CreateAsset<T>(string defaultName, string defaultPath) where T : ScriptableObject
        {
            string path = SavePanel(defaultName, defaultPath);
            // user click cancel
            if (string.IsNullOrEmpty(path)) { return null; }

            T asset = ScriptableObject.CreateInstance<T>();

            SaveAsset(path, asset);

            return asset;
        }

        static string SavePanel(string name, string defaultPath)
        {
            string path = EditorUtility.SaveFilePanel(
                           "Save ScriptableObject",
                           defaultPath,
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
