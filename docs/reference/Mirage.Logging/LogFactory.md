---
id: LogFactory
title: LogFactory
---

# Class LogFactory



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
public static class LogFactory
```


### Properties

#### Loggers

##### Declaration

```cs
public static IReadOnlyDictionary<string, ILogger> Loggers { get; }
```
### Methods
#### GetLogger&lt;T&gt;(LogType)



##### Declaration

```cs
public static ILogger GetLogger<T>(LogType defaultLogLevel = null)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| LogType | defaultLogLevel |  |

##### Returns
| Type | Description |
| ---- | ---- |
| ILogger |  |

#### GetLogger(Type, LogType)



##### Declaration

```cs
public static ILogger GetLogger(Type type, LogType defaultLogLevel = null)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Type | type |  |
| LogType | defaultLogLevel |  |

##### Returns
| Type | Description |
| ---- | ---- |
| ILogger |  |

#### GetLogger(String, LogType)



##### Declaration

```cs
public static ILogger GetLogger(string loggerName, LogType defaultLogLevel = null)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.String | loggerName |  |
| LogType | defaultLogLevel |  |

##### Returns
| Type | Description |
| ---- | ---- |
| ILogger |  |

#### ReplaceLogHandler(ILogHandler, Boolean)


Replacing log handlers for loggers, with the option to replace for exisitng or just new loggers



##### Declaration

```cs
public static void ReplaceLogHandler(ILogHandler logHandler, bool replaceExisting = true)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| ILogHandler | logHandler |  |
| System.Boolean | replaceExisting |  |


#### ReplaceLogHandler(Func&lt;String, ILogHandler&gt;, Boolean)


Replaceing log handlers for loggers, allows for unique log handlers for each type
this can be used to add labels or other processing before logging the result



##### Declaration

```cs
public static void ReplaceLogHandler(Func<string, ILogHandler> createHandler, bool replaceExisting = true)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Func&lt;System.String, ILogHandler&gt; | createHandler |  |
| System.Boolean | replaceExisting |  |


