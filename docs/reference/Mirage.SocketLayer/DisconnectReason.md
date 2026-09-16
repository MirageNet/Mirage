---
id: DisconnectReason
title: DisconnectReason
---

# Enum DisconnectReason


Reason why a connection was disconnected




##### Syntax

```cs
public enum DisconnectReason
```


### Fields

#### None

No reason given


##### Declaration

```cs
None = 0
```
#### Timeout

No message Received in timeout window


##### Declaration

```cs
Timeout = 1
```
#### RequestedByRemotePeer

Disconnect called by higher level


##### Declaration

```cs
RequestedByRemotePeer = 2
```
#### RequestedByLocalPeer

Disconnect called by higher level


##### Declaration

```cs
RequestedByLocalPeer = 3
```
#### InvalidPacket

Received packet was not allowed by config


##### Declaration

```cs
InvalidPacket = 4
```
#### SendBufferFull

Send buffer was full and could not accept more data


##### Declaration

```cs
SendBufferFull = 5
```
