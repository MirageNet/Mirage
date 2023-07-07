using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mirage.Logging;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mirage.EditorScripts.Logging
{
    public class LogLevelsGUI_UIElements : VisualElement
    {
        public static VisualElement Create(LogSettingsSO settings)
        {
            //var root = new VisualElement();
            //var title = new Label("Log Settings");
            //var soReference = new ObjectField("Settings");

            var element = new LogLevelsGUI_UIElements(settings);
            element.Draw();
            return element;
        }

        private LogSettingsSO settings;
        private LogSettingChecker checker;
        private Dictionary<string, bool> folderOutState = new Dictionary<string, bool>();
        private bool guiChanged;
        private LogType? _filter;
        // -1 => no type, will show as empty dropdown
        private const LogType NO_LEVEL = (LogType)(-1);

        public LogLevelsGUI_UIElements(LogSettingsSO settings)
        {
            this.settings = settings;
            checker = new LogSettingChecker(settings);

            style.paddingTop = 10;
        }

        public static void DrawSettings(LogSettingsSO settings)
        {
            var drawer = new LogLevelsGUI(settings);
            drawer.Draw();
            EditorUtility.SetDirty(settings);
        }

        public void Draw()
        {
            checker.Refresh();
            guiChanged = false;

            var helpText = "You may need to run your game a few times for this list to properly populate!";
            Add(new HelpBox(helpText, HelpBoxMessageType.Info));
            Add(CreateSetAllDropDown());
            Add(CreateFilterByDropDown());

            foreach (var group in GetGroups())
            {
                Add(CreateGroup(group));
            }

            Add(CreateDeleteAllButton());
            Add(CreateFindAllButton());
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

        private VisualElement CreateSetAllDropDown()
        {
            var container = new VisualElement();
            var allLogType = GetGroupLevel(settings.LogLevels);

            var label = new Label("Set All");
            container.Add(label);

            var enumField = new EnumField(allLogType);
            enumField.RegisterValueChangedCallback(e =>
            {
                SetGroupLevel(settings.LogLevels, (LogType)e.newValue);
                guiChanged = true;
            });
            container.Add(enumField);

            return container;
        }

        private VisualElement CreateFilterByDropDown()
        {
            var container = new VisualElement();

            var label = new Label("Filter by Level");
            container.Add(label);

            var enumField = new EnumField(_filter ?? LogType.Error);
            enumField.RegisterValueChangedCallback(e =>
            {
                if ((LogType)e.newValue == NO_LEVEL)
                    _filter = null;
                else
                    _filter = (LogType)e.newValue;

                guiChanged = true;
            });
            container.Add(enumField);

            var clearButton = new Button(() =>
            {
                _filter = null;
                guiChanged = true;
            });
            clearButton.text = "Clear Filter";
            container.Add(clearButton);

            return container;
        }

        private VisualElement CreateGroup(IGrouping<string, LogSettingsSO.LoggerSettings> group)
        {
            var container = new VisualElement();
            var namespaceName = string.IsNullOrEmpty(group.Key) ? "< no namespace >" : group.Key;

            if (!folderOutState.ContainsKey(namespaceName))
                folderOutState[namespaceName] = false;

            var foldout = new Foldout();
            foldout.text = namespaceName;
            foldout.value = folderOutState[namespaceName];
            foldout.RegisterValueChangedCallback(e =>
            {
                folderOutState[namespaceName] = e.newValue;
            });

            var foldoutContent = new VisualElement();
            foldout.Add(foldoutContent);

            foreach (var loggerType in group.OrderBy(x => x.Name))
            {
                var enumField = DrawNiceEnum(loggerType);
                foldoutContent.Add(enumField);
            }

            container.Add(foldout);
            return container;
        }

        private Button CreateDeleteAllButton()
        {
            var button = new Button(() =>
            {
                settings.LogLevels.Clear();
                LogFactory._loggers.Clear();
                guiChanged = true;
            });

            button.text = "Clear All Levels";
            return button;
        }

        private Button CreateFindAllButton()
        {
            var button = new Button(() =>
            {
                var flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;

                var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var asm in loadedAssemblies)
                {
                    foreach (var type in asm.GetTypes())
                    {
                        if (type.FullName.StartsWith("UnityEngine."))
                            continue;

                        if (type.IsGenericType)
                            continue;

                        foreach (var field in type.GetFields(flags))
                        {
                            try
                            {
                                if (field.IsStatic && field.FieldType == typeof(ILogger))
                                {
                                    var value = (ILogger)field.GetValue(null);
                                }
                            }
                            catch (Exception e)
                            {
                                Debug.LogError($"Failed to find Logger inside type {type.Name} with exception:{e}");
                            }
                        }
                    }
                }

                checker.Refresh();
                guiChanged = true;
            });

            button.text = "Find All Types Using Logger";
            return button;
        }

        private void ApplyAndSaveLevels()
        {
            foreach (var logSetting in settings.LogLevels)
            {
                var logger = LogFactory.GetLogger(logSetting.FullName);
                logger.filterLogType = logSetting.logLevel;
            }

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

        private static EnumField DrawNiceEnum(LogSettingsSO.LoggerSettings loggerType)
        {
            var name = loggerType.Name;
            var level = loggerType.logLevel;

            var enumField = new EnumField(ObjectNames.NicifyVariableName(name), level);
            enumField.RegisterValueChangedCallback(e =>
            {
                loggerType.logLevel = (LogType)e.newValue;
                // Add code to handle the value change if needed
            });

            return enumField;
        }
    }
}
