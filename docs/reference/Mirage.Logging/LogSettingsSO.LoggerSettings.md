---
id: LogSettingsSO.LoggerSettings
title: LogSettingsSO.LoggerSettings
---

# Class LogSettingsSO.LoggerSettings



<div class="inheritance">

##### Inheritance

<div class="level" style={{"--data-index": 0}}>
System.Object
</div>
</div>

##### Inherited Members

<details>
<summary>Show</summary>
</details>

##### Syntax

```cs
[Serializable]
public class LoggerSettings
```

### Constructors

#### LoggerSettings(String, String, LogType)



##### Declaration

```cs
public LoggerSettings(string name, string namespace, LogType level)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.String | name |  |
| System.String | namespace |  |
| LogType | level |  |

#### LoggerSettings(String, LogType)



##### Declaration

```cs
public LoggerSettings(string fullname, LogType level)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.String | fullname |  |
| LogType | level |  |

### Fields

#### Name

##### Declaration

```cs
public string Name
```
#### Namespace

##### Declaration

```cs
public string Namespace
```
#### logLevel

##### Declaration

```cs
public LogType logLevel
```

### Properties

#### FullName

##### Declaration

```cs
public string FullName { get; }
```
### Methods
#### GetNameAndNameSpaceFromFullname(String)



##### Declaration

```cs
public static (string name, string  namespace ) GetNameAndNameSpaceFromFullname(string fullname)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.String | fullname |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.ValueTuple{System.String,System.String} |  |

