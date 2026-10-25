---
id: NetworkFlags
title: NetworkFlags
---

# Enum NetworkFlags




##### Syntax

```cs
[Flags]
public enum NetworkFlags
```


### Fields

#### NotActive

If both server and client are not active. Can be used to check for singleplayer or unspawned object


##### Declaration

```cs
NotActive = 1
```
#### Server

##### Declaration

```cs
Server = 2
```
#### Client

##### Declaration

```cs
Client = 4
```
#### Active

If either Server or Client is active.

Note this will not check host mode. For host mode you need to use  and 



##### Declaration

```cs
Active = 6
```
#### HasAuthority

##### Declaration

```cs
HasAuthority = 8
```
#### LocalOwner

##### Declaration

```cs
LocalOwner = 16
```
