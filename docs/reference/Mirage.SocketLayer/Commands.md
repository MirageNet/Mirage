---
id: Commands
title: Commands
---

# Enum Commands


Small message used to control a connection

 and Commands uses their own byte/enum to split up the flow and add struture to the code.





##### Syntax

```cs
public enum Commands
```


### Fields

#### ConnectRequest

Sent from client to request to connect to server


##### Declaration

```cs
ConnectRequest = 1
```
#### ConnectionAccepted

Sent when Server accepts client


##### Declaration

```cs
ConnectionAccepted = 2
```
#### ConnectionRejected

Sent when server rejects client


##### Declaration

```cs
ConnectionRejected = 3
```
#### Disconnect

Sent from client or server to close connection


##### Declaration

```cs
Disconnect = 4
```
