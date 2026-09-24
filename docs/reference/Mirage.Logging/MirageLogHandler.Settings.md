---
id: MirageLogHandler.Settings
title: MirageLogHandler.Settings
---

# Class MirageLogHandler.Settings



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
public class Settings
```

### Constructors

#### Settings(MirageLogHandler.TimePrefix, Boolean, Boolean)



##### Declaration

```cs
public Settings(MirageLogHandler.TimePrefix timePrefix, bool coloredLabel, bool label)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Logging.MirageLogHandler.TimePrefix | timePrefix |  |
| System.Boolean | coloredLabel |  |
| System.Boolean | label |  |

#### Settings(Boolean, Boolean, Func&lt;String&gt;)



##### Declaration

```cs
public Settings(bool coloredLabel, bool label, Func<string> customTimePrefix)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Boolean | coloredLabel |  |
| System.Boolean | label |  |
| System.Func&lt;System.String&gt; | customTimePrefix |  |

### Fields

#### TimePrefix

##### Declaration

```cs
public MirageLogHandler.TimePrefix TimePrefix
```
#### ColoredLabel

##### Declaration

```cs
public readonly bool ColoredLabel
```
#### Label

##### Declaration

```cs
public readonly bool Label
```
#### ColorSeed

Used to change the colors of names
number is multiple by hash unchecked, so small changes to seed will cause large changes in result
403 seems like a good starting seed, common class like NetworkServer and NetworkClient have different colors


##### Declaration

```cs
public int ColorSeed
```
#### ColorSaturation

##### Declaration

```cs
public float ColorSaturation
```
#### ColorValue

##### Declaration

```cs
public float ColorValue
```
#### CustomTimePrefix

##### Declaration

```cs
public Func<string> CustomTimePrefix
```
### Methods
#### AllowColorToLabel(String, String)



##### Declaration

```cs
public string AllowColorToLabel(string fullname, string label)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.String | fullname |  |
| System.String | label |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.String |  |

#### ColorFromName(String)



##### Declaration

```cs
public Color ColorFromName(string fullName)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.String | fullName |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Color |  |

