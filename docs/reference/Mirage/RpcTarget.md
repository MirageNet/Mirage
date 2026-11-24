---
id: RpcTarget
title: RpcTarget
---

# Enum RpcTarget


Used by ClientRpc to tell mirage who to send remote call to




##### Syntax

```cs
public enum RpcTarget
```


### Fields

#### Owner

Sends to the  that owns the object


##### Declaration

```cs
Owner = 0
```
#### Observers

Sends to all  that can see the object


##### Declaration

```cs
Observers = 1
```
#### Player

Sends to the  that is given as an argument in the RPC function (requires target to be an observer)


##### Declaration

```cs
Player = 2
```
