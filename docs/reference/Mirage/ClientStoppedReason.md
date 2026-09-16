---
id: ClientStoppedReason
title: ClientStoppedReason
---

# Enum ClientStoppedReason


Reason why Client was stopped or disconnected




##### Syntax

```cs
[Serializable]
public enum ClientStoppedReason
```


### Fields

#### None
No reason given

##### Declaration

```cs
None = 0
```
#### Timeout
Connecting timed out
Server not sending replies

##### Declaration

```cs
Timeout = 1
```
#### LocalConnectionClosed
Connection disconnect called locally

##### Declaration

```cs
LocalConnectionClosed = 2
```
#### RemoteConnectionClosed
Connection disconnect called on server

##### Declaration

```cs
RemoteConnectionClosed = 3
```
#### InvalidPacket
Server disconnected because sent packet was not allowed by server config

##### Declaration

```cs
InvalidPacket = 8
```
#### SendBufferFull
Server disconnected because send buffer was full

##### Declaration

```cs
SendBufferFull = 10
```
#### ServerFull
Server rejected connecting because it was full

##### Declaration

```cs
ServerFull = 4
```
#### ConnectingTimeout
Server did not reply

##### Declaration

```cs
ConnectingTimeout = 5
```
#### ConnectingCancel
Disconnect called locally before server replies with connected

##### Declaration

```cs
ConnectingCancel = 6
```
#### KeyInvalid
Key given with first message did not match the value on the server, See SocketLayer Config

##### Declaration

```cs
KeyInvalid = 9
```
#### InvalidUnconnectedPacket

Send if  is true


##### Declaration

```cs
InvalidUnconnectedPacket = 11
```
#### HostModeStopped
Disconnect called when server was stopped in host mode

##### Declaration

```cs
HostModeStopped = 7
```
