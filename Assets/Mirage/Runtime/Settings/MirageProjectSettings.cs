using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mirage.Logging;
using UnityEngine;

namespace Mirage
{
    public class MirageProjectSettings : ScriptableObject
    {
        [Serializable]
        public struct Level : IEquatable<Level>
        {
            public string name;
            public LogType level;

            public bool Equals(Level other)
            {
                return name == other.name && level == other.level;
            }

            public override bool Equals(object obj)
            {
                return obj is Level other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (name.GetHashCode() * 397) ^ (int) level;
                }
            }

            public static bool operator ==(Level left, Level right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(Level left, Level right)
            {
                return !left.Equals(right);
            }
        }

#if UNITY_EDITOR
        // Don't change this
        public const string ROOT_FOLDER = "ProjectSettings/Packages/" + PACKAGE_NAME;
#endif

        // Change this
        public const string PACKAGE_NAME = "com.miragenet.mirage";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void InitializeLoggers()
        {
            MirageProjectSettings settings = Get();

            for (int i = 0; i < settings.logLevels.Count; i++)
            {
                ILogger logger = LogFactory.GetLogger(settings.logLevels[i].name);
                logger.filterLogType = settings.logLevels[i].level;
            }
        }

#if UNITY_EDITOR
        static MirageProjectSettings()
        {
            ProjectSettingsBuildProcessor.OnBuild += OnProjectSettingsBuild;
        }

        private static void OnProjectSettingsBuild(List<ScriptableObject> list, List<string> names)
        {
            list.Add(Get());
            names.Add(SettingName);
        }
#endif

        private static MirageProjectSettings settingsInstance;

        public static string SettingsPath
        {
            get
            {
#if UNITY_EDITOR
                return $"{ROOT_FOLDER}/{SettingName}.asset";
#else
                return $"{MirageProjectSettings.PACKAGE_NAME}/{SettingName}";
#endif
            }
        }

        private static string SettingName { get { return typeof(MirageProjectSettings).FullName; } }

        public static MirageProjectSettings Get()
        {
            if (settingsInstance != null)
            {
                return settingsInstance;
            }

#if UNITY_EDITOR
            string path = SettingsPath;

            if (!File.Exists(path))
            {
                settingsInstance = CreateInstance<MirageProjectSettings>();
                ProjectSettingsHelper.Save(settingsInstance, path);
            }
            else
            {
                settingsInstance = ProjectSettingsHelper.Load<MirageProjectSettings>(path);
            }

            settingsInstance.hideFlags = HideFlags.HideAndDontSave;
#else
            settingsInstance = Resources.Load<MirageProjectSettings>(SettingsPath);
#endif

#if UNITY_EDITOR
            var newLevels = new List<Level>();

            newLevels.AddRange(LogFactory.loggers.Select(kvp => new Level { name = kvp.Key, level = kvp.Value.filterLogType }));

            if (settingsInstance.logLevels == null || settingsInstance.logLevels.Count == 0)
            {
                settingsInstance.logLevels = newLevels;
                settingsInstance.logLevels.Sort((x, y) => String.Compare(x.name, y.name, StringComparison.Ordinal));
                settingsInstance.EditorSave();
            }
            else
            {
                bool dirty = false;
                for (int i = 0; i < newLevels.Count; i++)
                {
                    bool contains = false;
                    for (int j = 0; j < settingsInstance.logLevels.Count; j++)
                    {
                        if (settingsInstance.logLevels[j].name == newLevels[i].name)
                        {
                            contains = true;
                            break;
                        }
                    }

                    if (!contains)
                    {
                        settingsInstance.logLevels.Add(newLevels[i]);
                        dirty = true;
                    }
                }

                if (dirty)
                {
                    // Sort in order to keep them in the same place.
                    // It also looks nicer. :)
                    settingsInstance.logLevels.Sort((x, y) => String.Compare(x.name, y.name, StringComparison.Ordinal));
                    settingsInstance.EditorSave();
                }
            }
#endif

            return settingsInstance;
        }

#if UNITY_EDITOR
        public void EditorSave()
        {
            ProjectSettingsHelper.Save(settingsInstance, SettingsPath);
        }
#endif

        public List<Level> logLevels = null;
    }
}
