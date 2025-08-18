---
sidebar_position: 15
---
# Network Log Settings

Mirage's logging system provides granular control over log levels for different parts of your game, allowing you to easily manage the verbosity of messages from various Mirage components and your own custom code.

The logging system primarily uses two components:
*   **`LogSettings` (MonoBehaviour)**: A component you add to a GameObject (e.g., your NetworkManager) to load your logging configurations into the `LogFactory` at runtime.
*   **`LogSettingsSO` (ScriptableObject)**: An asset that stores the actual log level configurations for different loggers.

## `LogSettings` Component (MonoBehaviour)

The `LogSettings` component is a MonoBehaviour that you attach to a GameObject in your scene. Its primary role is to load the log level configurations defined in a `LogSettingsSO` asset into Mirage's `LogFactory` when your game starts or when the component is enabled. This ensures that your desired logging settings are applied at runtime.

When you first add a `LogSettings` component to a GameObject, you will need to assign a `LogSettingsSO` asset to its "Settings" field. If no `LogSettingsSO` asset exists in your project, you will be prompted to create a new one directly from the Inspector.

![Inspector With No Settings](/img/components/NetworkLogSettingsNoSettings.png)

:::note
If a `LogSettingsSO` asset already exists in your project, the `LogSettings` component will automatically assign it to the "Settings" field when it is added to a game object.
:::

## `LogSettingsSO` (ScriptableObject)

The `LogSettingsSO` is a ScriptableObject asset that stores the specific log level settings for various loggers within your project. Each logger corresponds to a specific class or namespace in your code.

When you first set up a new `LogSettingsSO` asset, the list of loggers may be empty or incomplete. Running your game will cause Mirage scripts to register their respective loggers with the `LogFactory`, populating this list so their logging levels can be changed.

## Configuring Log Levels in Unity Editor

You can configure the log levels for various Mirage components and your custom code directly within the Unity Editor. There are two primary ways to access these settings:

### Project Settings

The most common way to manage log settings is through Unity's Project Settings. Navigate to **Edit > Project Settings**, and then select **Mirage > Logging** from the left-hand menu.

Here, you can assign your `LogSettingsSO` asset. Once assigned, you will see a detailed interface that allows you to:
*   **Set All**: Change the log level for all listed loggers at once.
*   **Filter by Level**: Filter the displayed loggers by their current log level.
*   **Individual Logger Settings**: Adjust the log level for each specific logger (e.g., `Mirage.NetworkServer`, `Mirage.Client`).
*   **Namespaces**: Loggers are grouped by their namespace, allowing for easier navigation.
*   **Find All Type Using Logger**: This button will scan your project for types that use Mirage's `LogFactory.GetLogger()` method and add them to the list, ensuring you can control their log levels.

![Inspector](/img/components/NetworkLogSettings.png)

### Mirage Log Level Window

Alternatively, you can access the log settings through a dedicated window. Go to **Window > Analysis > Mirage Log Levels**. This window provides the same functionality as the Project Settings interface for managing your log levels.

![Window](/img/components/LogLevelWindow.png)

## Customizing Log Output with `MirageLogHandler`

Mirage provides a custom log handler, `MirageLogHandler`, which allows you to enhance your log messages with additional information like time prefixes and colored labels. This can be particularly useful for debugging and quickly identifying log entries from different parts of your application.

### Setting a Custom Log Handler

You can set `MirageLogHandler` as the default log handler for all Mirage loggers using the `LogFactory.ReplaceLogHandler()` method. This is typically done early in your application's lifecycle, for example, in an `Awake()` method of a MonoBehaviour or a static constructor.

Here's an example of how to set `MirageLogHandler` with default settings:

```csharp
using Mirage.Logging;
using UnityEngine;

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
```

### `MirageLogHandler` Settings

The `MirageLogHandler.Settings` class allows you to configure various aspects of the custom log output:

*   **`TimePrefix`**: Determines if and how a timestamp is added to log messages. Options include `None`, `FrameCount`, `UnscaledTime`, `DateTimeMilliSeconds`, and `DateTimeSeconds`.
*   **`ColoredLabel`**: If `true`, the label (logger name) will be colored based on a hash of the logger's full name, making it easier to distinguish logs from different sources.
*   **`Label`**: If `true`, a label (the logger's name) will be prepended to the log message.
*   **`ColorSeed`**: An integer used to seed the color generation for `ColoredLabel`. Changing this can alter the colors assigned to loggers.
*   **`ColorSaturation`**: Controls the saturation of the generated colors.
*   **`ColorValue`**: Controls the brightness (value) of the generated colors.

You can customize these settings when creating an instance of `MirageLogHandler.Settings` to tailor the log output to your preferences.

## Using `LogFactory` in Your Code

Mirage's logging system is designed to be easily integrated into your own game code, allowing you to leverage the same granular control over logging that Mirage's internal components use. You obtain an `ILogger` instance for your class or module using the `LogFactory`, and then use this logger to output messages.

### Obtaining an `ILogger`

To get an `ILogger` for your class, you typically declare a `static readonly` field at the top of your class. **`LogFactory.GetLogger<T>()` (or `LogFactory.GetLogger("YourCustomLoggerName")`) will always return the same `ILogger` instance for a given logger name, ensuring that any modifications to its settings (e.g., `filterLogType`) will apply consistently across your application.** Here's how:

```csharp
using Mirage.Logging; // Make sure to include this namespace
using UnityEngine;

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
        {
            logger.Log($"Current time: {Time.time}");
        }
    }
}
```

### Benefits

By using `LogFactory.GetLogger()`:
*   **Centralized Control**: All log messages from your code can be controlled via the `LogSettingsSO` asset in the Unity Editor, just like Mirage's internal logs.
*   **Consistency**: Ensures a consistent logging approach across your entire project.
*   **Performance**: You can check `logger.LogEnabled()` (or `WarnEnabled()`, `ErrorEnabled()`) before constructing complex log messages, preventing unnecessary string allocations when the log level is filtered out.

## Issues

Mirrors Logging API is currently a work in progress. If there is a bug or a feature you want to be added please make an issue [here](https://github.com/MirageNet/Mirage/issues).
