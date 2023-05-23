using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Mirage.Logging
{
    public class LogSettingsSO : ScriptableObject
    {
        public List<LoggerSettings> LogLevels = new List<LoggerSettings>();

        [Serializable]
        public class LoggerSettings
        {
            public string Name;
            public string Namespace;

            private string _fullNameCache;
            public string FullName
            {
                get
                {
                    // need lazy property here because unity deserializes without using constructor
                    if (string.IsNullOrEmpty(_fullNameCache))
                        _fullNameCache = CreateFullName(Name, Namespace);

                    return _fullNameCache;
                }
            }

            private static string CreateFullName(string name, string space)
            {
                // special case when namespace is null we just return null
                // see GetNameAndNameSpaceFromFullname
                if (string.IsNullOrEmpty(space))
                    return name;
                else
                    return $"{space}.{name}";
            }

            public LogType logLevel;

            public LoggerSettings(string name, string @namespace, LogType level)
            {
                // if string is null, use empty string instead
                Name = name ?? string.Empty;
                Namespace = @namespace ?? string.Empty;
                logLevel = level;
                _fullNameCache = CreateFullName(Name, Namespace);
            }
            public LoggerSettings(string fullname, LogType level)
            {
                (Name, Namespace) = GetNameAndNameSpaceFromFullname(fullname);
                logLevel = level;
                _fullNameCache = CreateFullName(Name, Namespace);
            }

            public static (string name, string @namespace) GetNameAndNameSpaceFromFullname(string fullname)
            {
                // NOTE we need to be able to recreate fullname from name/namespace
                // so we cant always just use empty string for no namespace

                if (!fullname.Contains("."))
                {
                    // if no `.` then return null
                    return (fullname, null);
                }

                var parts = fullname.Split('.');
                var name = parts.Last();

                string @namespace;
                if (parts.Length == 1)
                {
                    @namespace = string.Empty;
                }
                else
                {
                    @namespace = string.Join(".", parts.Take(parts.Length - 1));
                }

                Debug.Assert(CreateFullName(name, @namespace) == fullname, "Could not re-create full name from created parted");
                return (name, @namespace);
            }
        }
    }

    public static class LogSettingsExtensions
    {
        public static void SaveFromLogFactory(this LogSettingsSO settings)
        {
            var dictionary = LogFactory._loggers;
            if (settings == null)
            {
                Debug.LogWarning("Could not SaveFromDictionary because LogSettings were null");
                return;
            }

            settings.LogLevels.Clear();

            foreach (var kvp in dictionary)
            {
                settings.LogLevels.Add(new LogSettingsSO.LoggerSettings(kvp.Key, kvp.Value.filterLogType));
            }

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(settings);
#endif
        }

        public static void LoadIntoLogFactory(this LogSettingsSO settings)
        {
            if (settings == null)
            {
                Debug.LogWarning("Could not LoadIntoDictionary because LogSettings were null");
                return;
            }

            for (var i = 0; i < settings.LogLevels.Count; i++)
            {
                var logLevel = settings.LogLevels[i];
                var key = logLevel.FullName;
                if (key == null)
                {
                    settings.LogLevels.RemoveAt(i);
                    i--;
                    Debug.LogWarning("Found null key in log settings, removing item");
                    continue;
                }

                var logger = LogFactory.GetLogger(key);
                logger.filterLogType = logLevel.logLevel;
            }
        }
    }
}
