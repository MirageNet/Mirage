# Network Log Settings

**See also [LogFactory](/docs/reference/Mirage.Logging/LogFactory) in the API Reference.**

## Network Log Settings component

The Network Log Settings component allows you to configure logging levels and load the settings in a build.

When you first add NetworkLogSettings you will have to Create a new LogSettings asset that will store the settings.

![Inspector With No Settings](/img/components/NetworkLogSettingsNoSettings.png)

:::note
If a LogSettings asset already exists the NetworkLogSettings component will set the Settings field when it is added to a game object.
:::

## Log Settings 

When you first set up LogSettings the list of components may be empty or incomplete. Running the game will cause Mirage scripts to add their respective loggers to the list so their logging levels can be changed.

![Inspector](/img/components/NetworkLogSettings.png)

Log settings can also be changed using the "Mirage Log Level" window, which can be opened from the editor menu: Window > Analysis > Mirage Log Levels.

![Window](/img/components/LogLevelWindow.png)

To change settings at runtime please see [LogFactory](/docs/reference/Mirage.Logging/LogFactory).

## Issues

Mirrors Logging API is currently a work in progress. If there is a bug or a feature you want to be added please make an issue [here](https://github.com/MirageNet/Mirage/issues).
