---
id: ILoggerExtensions
title: ILoggerExtensions
---

# Class ILoggerExtensions



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
public static class ILoggerExtensions
```

### Methods
#### LogError(ILogger, Object)



##### Declaration

```cs
public static void LogError(this ILogger logger, object message)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| ILogger | logger |  |
| System.Object | message |  |


#### Assert(ILogger, Boolean, Object)



##### Declaration

```cs
[Conditional("UNITY_ASSERTIONS")]
public static void Assert(this ILogger logger, bool condition, object message)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| ILogger | logger |  |
| System.Boolean | condition |  |
| System.Object | message |  |


#### Assert(ILogger, Boolean)



##### Declaration

```cs
[Conditional("UNITY_ASSERTIONS")]
public static void Assert(this ILogger logger, bool condition)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| ILogger | logger |  |
| System.Boolean | condition |  |


#### LogWarning(ILogger, Object)



##### Declaration

```cs
public static void LogWarning(this ILogger logger, object message)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| ILogger | logger |  |
| System.Object | message |  |


#### LogEnabled(ILogger)



##### Declaration

```cs
public static bool LogEnabled(this ILogger logger)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| ILogger | logger |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean |  |

#### WarnEnabled(ILogger)



##### Declaration

```cs
public static bool WarnEnabled(this ILogger logger)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| ILogger | logger |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean |  |

#### ErrorEnabled(ILogger)



##### Declaration

```cs
public static bool ErrorEnabled(this ILogger logger)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| ILogger | logger |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean |  |

