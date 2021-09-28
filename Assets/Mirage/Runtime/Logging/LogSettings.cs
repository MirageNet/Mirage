using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Mirage.Logging
{
    public class LogSettings : ScriptableObject
    {
        public List<LoggerSettings> LogLevels = new List<LoggerSettings>();

        [Serializable]
        public class LoggerSettings
        {
            public string Name;
            public string Namespace;
            public string FullName => $"{Namespace}.{Name}";

            public LogType logLevel;

            public LoggerSettings(string name, string Namespace, LogType level)
            {
                Name = name;
                this.Namespace = Namespace;
                logLevel = level;
            }
            public LoggerSettings(string fullname, LogType level)
            {
                (Name, Namespace) = GetNameAndNameSapceFromFullname(fullname);
                logLevel = level;
            }

            private static (string name, string @namespace) GetNameAndNameSapceFromFullname(string fullname)
            {
                string[] parts = fullname.Split('.');
                string name = parts.Last();

                string @namespace;
                if (parts.Length == 1)
                {
                    @namespace = string.Empty;
                }
                else
                {
                    @namespace = string.Join(".", parts.Take(parts.Length - 1));
                }

                return (name, @namespace);
            }
        }
    }

    public static class LogSettingsExtensions
    {
        public static void SaveFromDictionary(this LogSettings settings, Dictionary<string, ILogger> dictionary)
        {
            if (settings == null)
            {
                Debug.LogWarning("Could not SaveFromDictionary because LogSettings were null");
                return;
            }

            settings.LogLevels.Clear();

            foreach (KeyValuePair<string, ILogger> kvp in dictionary)
            {
                settings.LogLevels.Add(new LogSettings.LoggerSettings(kvp.Key, kvp.Value.filterLogType));
            }

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(settings);
#endif
        }

        public static void LoadIntoDictionary(this LogSettings settings, Dictionary<string, ILogger> dictionary)
        {
            if (settings == null)
            {
                Debug.LogWarning("Could not LoadIntoDictionary because LogSettings were null");
                return;
            }

            foreach (LogSettings.LoggerSettings logLevel in settings.LogLevels)
            {
                if (dictionary.TryGetValue(logLevel.FullName, out ILogger logger))
                {
                    logger.filterLogType = logLevel.logLevel;
                }
                else
                {
                    logger = new Logger(Debug.unityLogger)
                    {
                        filterLogType = logLevel.logLevel
                    };

                    dictionary[logLevel.FullName] = logger;
                }
            }
        }
    }
}
