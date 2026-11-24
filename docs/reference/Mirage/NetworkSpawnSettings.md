---
id: NetworkSpawnSettings
title: NetworkSpawnSettings
---

# Struct NetworkSpawnSettings


Spawn Settings for 




##### Syntax

```cs
[Serializable]
public struct NetworkSpawnSettings
```

### Constructors

#### NetworkSpawnSettings(Boolean, Boolean, Boolean, Boolean, SyncActiveOption)



##### Declaration

```cs
public NetworkSpawnSettings(bool sendPosition, bool sendRotation, bool sendScale, bool sendName, SyncActiveOption sendActive)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Boolean | sendPosition |  |
| System.Boolean | sendRotation |  |
| System.Boolean | sendScale |  |
| System.Boolean | sendName |  |
| Mirage.SyncActiveOption | sendActive |  |

#### NetworkSpawnSettings(Boolean, Boolean, Boolean)



##### Declaration

```cs
public NetworkSpawnSettings(bool sendPosition, bool sendRotation, bool sendScale)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Boolean | sendPosition |  |
| System.Boolean | sendRotation |  |
| System.Boolean | sendScale |  |

### Fields

#### SendPosition

##### Declaration

```cs
public bool SendPosition
```
#### SendRotation

##### Declaration

```cs
public bool SendRotation
```
#### SendScale

##### Declaration

```cs
public bool SendScale
```
#### SendName

##### Declaration

```cs
public bool SendName
```
#### SendActive

##### Declaration

```cs
public SyncActiveOption SendActive
```

### Properties

#### Default

##### Declaration

```cs
public static NetworkSpawnSettings Default { get; }
```
