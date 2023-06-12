using System;
using Mirage.Logging;
using UnityEngine;

namespace Mirage.Tests.Runtime
{
    /// <summary>
    /// Used to turn logs off or on for test, and then reset after by using using block
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LogOverride<T> : IDisposable
    {
        private readonly ILogger _logger;
        private readonly LogType _startingLevel;

        public LogOverride(LogType overrideLevel = LogType.Exception)
        {
            _logger = LogFactory.GetLogger<T>();
            _startingLevel = _logger.filterLogType;

            _logger.filterLogType = overrideLevel;
        }
        public void Dispose()
        {
            _logger.filterLogType = _startingLevel;
        }
    }
}
