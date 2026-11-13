---
id: ConnectionState
title: ConnectionState
---

# Enum ConnectionState




##### Syntax

```cs
public enum ConnectionState
```


### Fields

#### Created

Initial state


##### Declaration

```cs
Created = 1
```
#### Connecting

Client is connecting to server


##### Declaration

```cs
Connecting = 2
```
#### Connected

Server as accepted connection


##### Declaration

```cs
Connected = 3
```
#### Disconnected

Server or client has disconnected the connection and is waiting to be cleaned up


##### Declaration

```cs
Disconnected = 9
```
#### Removing

Marked to be removed from the connection collection


##### Declaration

```cs
Removing = 10
```
#### Destroyed

Removed from collection and all state cleaned up


##### Declaration

```cs
Destroyed = 11
```
