---
id: Metrics.Frame
title: Metrics.Frame
---

# Struct Metrics.Frame




##### Syntax

```cs
public struct Frame
```


### Fields

#### init
Is this frame initialized (uninitialized frames can be excluded from averages)

##### Declaration

```cs
public bool init
```
#### connectionCount
Number of connections

##### Declaration

```cs
public int connectionCount
```
#### sendCount
Number of send calls to connections

##### Declaration

```cs
public int sendCount
```
#### sendBytes
Number of bytes sent to connections

##### Declaration

```cs
public int sendBytes
```
#### resendCount
Number of resend calls by reliable system

##### Declaration

```cs
public int resendCount
```
#### resendBytes
Number of bytes resent by reliable system

##### Declaration

```cs
public int resendBytes
```
#### receiveCount
Number of packets received from connections

##### Declaration

```cs
public int receiveCount
```
#### receiveBytes
Number of bytes received from connections

##### Declaration

```cs
public int receiveBytes
```
#### sendUnconnectedCount
Number of send calls to unconnected addresses

##### Declaration

```cs
public int sendUnconnectedCount
```
#### sendUnconnectedBytes
Number of bytes sent to unconnected addresses

##### Declaration

```cs
public int sendUnconnectedBytes
```
#### receiveUnconnectedBytes
Number of packets received from unconnected addresses

##### Declaration

```cs
public int receiveUnconnectedBytes
```
#### receiveUnconnectedCount
Number of bytes received from unconnected addresses

##### Declaration

```cs
public int receiveUnconnectedCount
```
#### sendMessagesUnreliableCount
Number of Unreliable message sent to connections

##### Declaration

```cs
public int sendMessagesUnreliableCount
```
#### sendMessagesUnreliableBytes
Number of Unreliable bytes sent to connections (excludes packets headers, will just be the message sent by high level)

##### Declaration

```cs
public int sendMessagesUnreliableBytes
```
#### receiveMessagesUnreliableCount
Number of Unreliable message received from connections

##### Declaration

```cs
public int receiveMessagesUnreliableCount
```
#### receiveMessagesUnreliableBytes
Number of Unreliable bytes received from connections (excludes packets headers, will just be the message sent by high level)

##### Declaration

```cs
public int receiveMessagesUnreliableBytes
```
#### sendMessagesReliableCount
Number of Reliable message sent to connections

##### Declaration

```cs
public int sendMessagesReliableCount
```
#### sendMessagesReliableBytes
Number of Reliable bytes sent to connections (excludes packets headers, will just be the message sent by high level)

##### Declaration

```cs
public int sendMessagesReliableBytes
```
#### receiveMessagesReliableCount
Number of Reliable message received from connections

##### Declaration

```cs
public int receiveMessagesReliableCount
```
#### receiveMessagesReliableBytes
Number of Reliable bytes received from connections (excludes packets headers, will just be the message sent by high level)

##### Declaration

```cs
public int receiveMessagesReliableBytes
```
#### sendMessagesNotifyCount
Number of Notify message sent to connections

##### Declaration

```cs
public int sendMessagesNotifyCount
```
#### sendMessagesNotifyBytes
Number of Notify bytes sent to connections (excludes packets headers, will just be the message sent by high level)

##### Declaration

```cs
public int sendMessagesNotifyBytes
```
#### receiveMessagesNotifyCount
Number of Notify message received from connections

##### Declaration

```cs
public int receiveMessagesNotifyCount
```
#### receiveMessagesNotifyBytes
Number of Notify bytes received from connections (excludes packets headers, will just be the message sent by high level)

##### Declaration

```cs
public int receiveMessagesNotifyBytes
```

### Properties

#### sendMessagesCountTotal
Number of message sent to connections

##### Declaration

```cs
public int sendMessagesCountTotal { get; }
```
#### sendMessagesBytesTotal
Number of bytes sent to connections (excludes packets headers, will just be the message sent by high level)

##### Declaration

```cs
public int sendMessagesBytesTotal { get; }
```
#### receiveMessagesCountTotal
Number of message received from connections

##### Declaration

```cs
public int receiveMessagesCountTotal { get; }
```
#### receiveMessagesBytesTotal
Number of bytes received from connections (excludes packets headers, will just be the message sent by high level)

##### Declaration

```cs
public int receiveMessagesBytesTotal { get; }
```
