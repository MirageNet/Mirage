---
id: SyncActiveOption
title: SyncActiveOption
---

# Enum SyncActiveOption




##### Syntax

```cs
public enum SyncActiveOption
```


### Fields

#### DoNothing

Do nothing - leave the game object in its current state.


##### Declaration

```cs
DoNothing = 0
```
#### SyncWithServer

Synchronize the active state of the game object with the server&apos;s state.


##### Declaration

```cs
SyncWithServer = 1
```
#### ForceEnable

Force-enable the game object, even if the server&apos;s version is disabled.


##### Declaration

```cs
ForceEnable = 2
```
