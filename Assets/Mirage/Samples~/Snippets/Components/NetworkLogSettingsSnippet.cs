using Mirage.Logging;
using UnityEngine;

namespace Mirage.Snippets.Components
{
    // CodeEmbed-Start: custom-log-setup
    public class CustomLogSetup : MonoBehaviour
    {
        void Awake()
        {
            // Create default settings for MirageLogHandler
            var settings = new MirageLogHandler.Settings(
                timePrefix: MirageLogHandler.TimePrefix.DateTimeMilliSeconds,
                coloredLabel: true,
                label: true
            );

            // Replace the default log handler with MirageLogHandler
            // This will apply to all existing and future loggers
            LogFactory.ReplaceLogHandler((loggerName) => new MirageLogHandler(settings, loggerName));
        }
    }
    // CodeEmbed-End: custom-log-setup

    // CodeEmbed-Start: my-game-manager
    public class MyGameManager : MonoBehaviour
    {
        // Obtain a logger for this class.
        // The LogFactory will automatically manage its log level based on your LogSettingsSO.
        private static readonly ILogger logger = LogFactory.GetLogger<MyGameManager>();

        void Start()
        {
            // Example usage of the logger
            logger.Log("MyGameManager started!");
            logger.LogWarning("Something might be wrong here.");
            logger.LogError("Critical error occurred!");

            // You can also check if a log type is enabled before logging to avoid unnecessary string formatting
            if (logger.LogEnabled())
                logger.Log($"Current time: {Time.time}");
        }
    }
    // CodeEmbed-End: my-game-manager
}
