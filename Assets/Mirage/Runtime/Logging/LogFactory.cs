using System;
using System.Collections.Generic;
using System.Linq;
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
        private static Func<string, ILogHandler> createLoggerForType = _ => Debug.unityLogger;

        public static ILogger GetLogger<T>(LogType defaultLogLevel = LogType.Warning)
        {
            return GetLogger(typeof(T), defaultLogLevel);
        }

        public static ILogger GetLogger(System.Type type, LogType defaultLogLevel = LogType.Warning)
        {
            // Full name for generic type is messy, instead
            if (type.IsGenericType && !type.IsGenericTypeDefinition)
            {
                var genericArgs = string.Join(",", type.GetGenericArguments().Select(x => x.Name));
                // remove `1 from end of name
                var name = type.Name.Substring(0, type.Name.IndexOf('`'));

                return GetLogger($"{type.Namespace}.{name}<{genericArgs}>", defaultLogLevel);
            }
            else
            {
                return GetLogger(type.FullName, defaultLogLevel);
            }
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
            var logger = new Logger(createLoggerForType.Invoke(loggerName))
            {
                // by default, log warnings and up
                filterLogType = defaultLogLevel
            };

            _loggers[loggerName] = logger;
            return logger;
        }

        /// <summary>
        /// Replacing log handlers for loggers, with the option to replace for exisitng or just new loggers
        /// </summary>
        /// <param name="logHandler"></param>
        public static void ReplaceLogHandler(ILogHandler logHandler, bool replaceExisting = true)
        {
            ReplaceLogHandler(_ => logHandler, replaceExisting);
        }

        /// <summary>
        /// Replaceing log handlers for loggers, allows for unique log handlers for each type
        /// <para>this can be used to add labels or other processing before logging the result</para>
        /// </summary>
        /// <param name="createHandler"></param>
        /// <param name="replaceExisting"></param>
        public static void ReplaceLogHandler(Func<string, ILogHandler> createHandler, bool replaceExisting = true)
        {
            createLoggerForType = createHandler;

            if (replaceExisting)
            {
                foreach (var kvp in _loggers)
                {
                    var logger = kvp.Value;
                    var key = kvp.Key;
                    logger.logHandler = createLoggerForType.Invoke(key);
                }
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
