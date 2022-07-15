using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Mirage.Logging
{
    public static class LogFactory
    {
        internal static readonly Dictionary<string, ILogger> _loggers = new Dictionary<string, ILogger>();

        public static IReadOnlyDictionary<string, ILogger> Loggers => _loggers;

        /// <summary>
        /// logHandler used for new loggers
        /// </summary>
        private static ILogHandler defaultLogHandler = Debug.unityLogger;

        public static ILogger GetLogger<T>(LogType defaultLogLevel = LogType.Warning)
        {
            return GetLogger(typeof(T).FullName, defaultLogLevel);
        }

        public static ILogger GetLogger(System.Type type, LogType defaultLogLevel = LogType.Warning)
        {
            return GetLogger(type.FullName, defaultLogLevel);
        }

        public static ILogger GetLogger(string loggerName, LogType defaultLogLevel = LogType.Warning)
        {
            if (_loggers.TryGetValue(loggerName, out var logger))
            {
                return logger;
            }

            return CreateNewLogger(loggerName, defaultLogLevel);
        }

        private static ILogger CreateNewLogger(string loggerName, LogType defaultLogLevel)
        {
            var logger = new Logger(defaultLogHandler)
            {
                // by default, log warnings and up
                filterLogType = defaultLogLevel
            };

            _loggers[loggerName] = logger;
            return logger;
        }

        /// <summary>
        /// Replacing log handler for all existing loggers and sets defaultLogHandler for new loggers
        /// </summary>
        /// <param name="logHandler"></param>
        public static void ReplaceLogHandler(ILogHandler logHandler)
        {
            defaultLogHandler = logHandler;

            foreach (var logger in _loggers.Values)
            {
                logger.logHandler = logHandler;
            }
        }
    }


    public static class ILoggerExtensions
    {
        public static void LogError(this ILogger logger, object message)
        {
            logger.Log(LogType.Error, message);
        }

        [System.Diagnostics.Conditional("UNITY_ASSERTIONS")]
        public static void Assert(this ILogger logger, bool condition, object message)
        {
            if (!condition)
                logger.Log(LogType.Assert, message);
        }

        [System.Diagnostics.Conditional("UNITY_ASSERTIONS")]
        public static void Assert(this ILogger logger, bool condition)
        {
            if (!condition)
                logger.Log(LogType.Assert, "Failed Assertion");
        }

        public static void LogWarning(this ILogger logger, object message)
        {
            logger.Log(LogType.Warning, message);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool LogEnabled(this ILogger logger) => logger.IsLogTypeAllowed(LogType.Log);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool WarnEnabled(this ILogger logger) => logger.IsLogTypeAllowed(LogType.Warning);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ErrorEnabled(this ILogger logger) => logger.IsLogTypeAllowed(LogType.Error);
    }
}
