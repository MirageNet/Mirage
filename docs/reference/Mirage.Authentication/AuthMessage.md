---
id: AuthMessage
title: AuthMessage
---

# Struct AuthMessage


Wrapper message around auth message sent by a 

This type is used to that it can be receive before player is authenticated.
ALl AuthMessage will be handled by an Authenticator instead of the normal message handler





##### Syntax

```cs
public struct AuthMessage
```


### Fields

#### Payload

##### Declaration

```cs
public ArraySegment<byte> Payload
```
