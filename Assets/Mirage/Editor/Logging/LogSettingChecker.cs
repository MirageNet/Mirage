using System.Collections.Generic;
using Mirage.Logging;
using UnityEngine;

namespace Mirage.EditorScripts.Logging
{
    /// <summary>
    /// Removes duplicates and updates log settings from LogFactory
    /// </summary>
    public class LogSettingChecker
    {
        private readonly LogSettingsSO settings;
        private readonly HashSet<string> duplicateChecker = new HashSet<string>();

        public LogSettingChecker(LogSettingsSO settings)
        {
            this.settings = settings;
        }

        public void Refresh()
        {
            duplicateChecker.Clear();
            RemoveDuplicates();
            AddNewFromFactory();
        }

        private void RemoveDuplicates()
        {
            for (int i = 0; i < settings.LogLevels.Count; i++)
            {
                bool added = duplicateChecker.Add(settings.LogLevels[i].FullName);
                // is duplicate, remove it
                if (!added)
                {
                    settings.LogLevels.RemoveAt(i);
                    i--;
                }
            }
        }

        private void AddNewFromFactory()
        {
            // try add new types
            foreach (KeyValuePair<string, ILogger> logger in LogFactory.Loggers)
            {
                bool added = duplicateChecker.Add(logger.Key);
                // is new, add it
                if (added)
                {
                    settings.LogLevels.Add(new LogSettingsSO.LoggerSettings(logger.Key, logger.Value.filterLogType));
                }
            }
        }
    }
}
