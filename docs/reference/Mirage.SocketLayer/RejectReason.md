---
id: RejectReason
title: RejectReason
---

# Enum RejectReason


Reason for reject sent from server




##### Syntax

```cs
public enum RejectReason
```


### Fields

#### None

No reason given


##### Declaration

```cs
None = 0
```
#### ServerFull

Server is at max connections and will not accept a new connection until one disconnects


##### Declaration

```cs
ServerFull = 1
```
#### Timeout

Server did not reply to connection request 


##### Declaration

```cs
Timeout = 2
```
#### ClosedByPeer

Closed called locally before connect


##### Declaration

```cs
ClosedByPeer = 3
```
#### KeyInvalid

Key given with first message did not match the value on the server


##### Declaration

```cs
KeyInvalid = 4
```
#### InvalidUnconnectedPacket

Send if  is true


##### Declaration

```cs
InvalidUnconnectedPacket = 5
```
