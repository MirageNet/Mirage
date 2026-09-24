---
id: NetworkDiagnostics.MessageInfo
title: NetworkDiagnostics.MessageInfo
---

# Struct NetworkDiagnostics.MessageInfo


Describes an outgoing message




##### Syntax

```cs
public struct MessageInfo
```

### Constructors

#### MessageInfo(Object, Int32, Int32)



##### Declaration

```cs
public MessageInfo(object message, int bytes, int count)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Object | message |  |
| System.Int32 | bytes |  |
| System.Int32 | count |  |

### Fields

#### message

The message being sent


##### Declaration

```cs
public readonly object message
```
#### bytes

how big was the message (does not include transport headers)


##### Declaration

```cs
public readonly int bytes
```
#### count

How many connections was the message sent to
If an object has a lot of observers this count could be high


##### Declaration

```cs
public readonly int count
```
