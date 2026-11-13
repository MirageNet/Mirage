---
id: MirageLogHandler
title: MirageLogHandler
---

# Class MirageLogHandler


Log handler that adds prefixes to logging



<div class="inheritance">

##### Inheritance

<div class="level" style={{"--data-index": 0}}>
System.Object
</div>
</div>

##### Syntax

```cs
public class MirageLogHandler : ILogHandler
```

### Constructors

#### MirageLogHandler(MirageLogHandler.Settings, String, ILogHandler)



##### Declaration

```cs
public MirageLogHandler(MirageLogHandler.Settings settings, string fullTypeName = null, ILogHandler inner = null)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Logging.MirageLogHandler.Settings | settings |  |
| System.String | fullTypeName |  |
| ILogHandler | inner |  |
### Methods
#### LogException(Exception, UnityEngine.Object)



##### Declaration

```cs
public void LogException(Exception exception, UnityEngine.Object context)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Exception | exception |  |
| UnityEngine.Object | context |  |


#### LogFormat(LogType, UnityEngine.Object, String, Object[])



##### Declaration

```cs
public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| LogType | logType |  |
| UnityEngine.Object | context |  |
| System.String | format |  |
| System.Object[] | args |  |


