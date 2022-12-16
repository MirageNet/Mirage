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

        // -1 => no type, will show as empty dropdown
        private const LogType NO_LEVEL = (LogType)(-1);

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
                var newSettings = ScriptableObjectUtility.CreateAsset<LogSettingsSO>(nameof(LogSettingsSO), "Assets");
                newSettings.SaveFromLogFactory();
                return newSettings;
            }

            return null;
        }

        private readonly LogSettingsSO settings;
        private readonly LogSettingChecker checker;
        private readonly Dictionary<string, bool> folderOutState = new Dictionary<string, bool>();

        /// <summary>
        /// Keep track of gui changed. If it has changed then we need to update <see cref="LogFactory"/> and save the new levels to file
        /// </summary>
        private bool guiChanged;
        private LogType? _filter;

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

                DrawSetAllDropDown();

                EditorGUILayout.Space();

                DrawFilterByDropDown();

                EditorGUILayout.Space();

                foreach (var group in GetGroups())
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

        private IOrderedEnumerable<IGrouping<string, LogSettingsSO.LoggerSettings>> GetGroups()
        {
            IEnumerable<LogSettingsSO.LoggerSettings> levels = settings.LogLevels;
            if (_filter.HasValue)
            {
                var filter = _filter.Value;
                levels = levels.Where(x => x.logLevel == filter);
            }

            return levels.GroupBy(x => x.Namespace).OrderBy(x => x.Key);
        }

        private void DrawSetAllDropDown()
        {
            using (var scope = new EditorGUI.ChangeCheckScope())
            {
                var allLogType = GetGroupLevel(settings.LogLevels);
                allLogType = (LogType)EditorGUILayout.EnumPopup("Set All", allLogType);
                if (scope.changed)
                {
                    SetGroupLevel(settings.LogLevels, allLogType);
                }
            }
        }
        private void DrawFilterByDropDown()
        {
            var result = (LogType)EditorGUILayout.EnumPopup("filter by level", _filter ?? NO_LEVEL);

            if (result == NO_LEVEL)
                _filter = null;
            else
                _filter = result;

            if (GUILayout.Button("Clear filter", GUILayout.Width(100)))
            {
                _filter = null;
            }
        }

        private void DrawGroup(IGrouping<string, LogSettingsSO.LoggerSettings> group)
        {
            var NameSpace = string.IsNullOrEmpty(group.Key) ? "< no namespace >" : group.Key;
            if (!folderOutState.ContainsKey(NameSpace))
                folderOutState[NameSpace] = false;

            folderOutState[NameSpace] = EditorGUILayout.Foldout(folderOutState[NameSpace], NameSpace, toggleOnLabelClick: true, EditorStyles.foldoutHeader);


            if (folderOutState[NameSpace])
            {
                EditorGUI.indentLevel++;
                foreach (var loggerType in group.OrderBy(x => x.Name))
                {
                    using (var scope = new EditorGUI.ChangeCheckScope())
                    {
                        var level = DrawNiceEnum(loggerType);

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
            GUILayout.Label("NOTES: when clearing it might require assembly to be reloaded before the 'find all' button will find everything");
            if (GUILayout.Button("Clear All levels"))
            {
                settings.LogLevels.Clear();
                LogFactory._loggers.Clear();
                guiChanged = true;
            }
        }

        private void DrawFindAllButton()
        {
            if (GUILayout.Button("Find All type using logger"))
            {
                var flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;

                var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var asm in loadedAssemblies)
                {
                    foreach (var type in asm.GetTypes())
                    {
                        // skip unity so that we dont fine Debug.Logger
                        if (type.FullName.StartsWith("UnityEngine."))
                            continue;

                        // Can't load fields for generic types
                        if (type.IsGenericType)
                            continue;

                        foreach (var field in type.GetFields(flags))
                        {
                            try
                            {
                                if (field.IsStatic && field.FieldType == typeof(ILogger))
                                {
                                    // will cause static field to initialize and call GetLogger
                                    // this will get existing or add new logger to factory
                                    var value = (ILogger)field.GetValue(null);

                                    // we dont then need to add it to factory manually, because the above should have added it
                                    // this is better than adding manually because AddLogger might add using string instead of types full name
                                }
                            }
                            catch (Exception e)
                            {
                                Debug.LogError($"Failed to find Logger inside type {type.Name} with exception:{e}");
                            }
                        }
                    }
                }

                // refresh so settings list has new items from Factory
                checker.Refresh();
                guiChanged = true;
            }
        }

        private void ApplyAndSaveLevels()
        {
            foreach (var logSetting in settings.LogLevels)
            {
                var logger = LogFactory.GetLogger(logSetting.FullName);
                logger.filterLogType = logSetting.logLevel;
            }

            // todo save outside of editor
            EditorUtility.SetDirty(settings);
        }

        private LogType GetGroupLevel(IEnumerable<LogSettingsSO.LoggerSettings> group)
        {
            if (!group.Any()) { return LogType.Warning; }

            var distinctLevels = group.Select(x => x.logLevel).Distinct();
            var allSame = distinctLevels.Count() == 1;

            if (allSame)
            {
                return distinctLevels.First();
            }
            else
            {
                return NO_LEVEL;
            }
        }
        private void SetGroupLevel(IEnumerable<LogSettingsSO.LoggerSettings> group, LogType level)
        {
            foreach (var logger in group)
            {
                logger.logLevel = level;
            }
        }

        private static LogType DrawNiceEnum(LogSettingsSO.LoggerSettings loggerType)
        {
            var name = loggerType.Name;
            var level = loggerType.logLevel;

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
            var path = SavePanel(defaultName, defaultPath);
            // user click cancel
            if (string.IsNullOrEmpty(path)) { return null; }

            var asset = ScriptableObject.CreateInstance<T>();

            SaveAsset(path, asset);

            return asset;
        }

        private static string SavePanel(string name, string defaultPath)
        {
            var path = EditorUtility.SaveFilePanel(
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

        private static void SaveAsset(string path, ScriptableObject asset)
        {
            var assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path);

            AssetDatabase.CreateAsset(asset, assetPathAndName);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
