---
id: PlayerErrorFlags
title: PlayerErrorFlags
---

# Enum PlayerErrorFlags




##### Syntax

```cs
[Flags]
public enum PlayerErrorFlags
```


### Fields

#### None
No custom errors code set.

##### Declaration

```cs
None = 0
```
#### RpcNullException
Rpc function threw  or . Likely logic error in code

##### Declaration

```cs
RpcNullException = 1
```
#### RpcException
Rpc function threw . Likely logic error in code

##### Declaration

```cs
RpcException = 2
```
#### DeserializationException
NetworkReader threw . More likely to be out of sync version than logic error, but could be caused by custom reader.

##### Declaration

```cs
DeserializationException = 4
```
#### RpcSync
Rpc index or message type was wrong. This could be from out-of-date build.

##### Declaration

```cs
RpcSync = 8
```
#### RateLimit
User hit a rate limit, rather than causing a direct error

##### Declaration

```cs
RateLimit = 16
```
#### InvalidState
Player performed an action that is not allowed due to invalid state. e.g. sending a reply for an RPC that is no longer pending.

##### Declaration

```cs
InvalidState = 512
```
#### SerializationLimit
Player sent a payload exceeding maximum size limit for string or collection.

##### Declaration

```cs
SerializationLimit = 1024
```
#### NoAuthority
Player does not have authority to call this object. Could happen in normal gameplay if changing the owner of an object.

##### Declaration

```cs
NoAuthority = 32
```
#### Unauthenticated
Message send before Authentication is complete

##### Declaration

```cs
Unauthenticated = 64
```
#### Critical
Error was critical, should be used to indicate player should be kicked/timed out/banned

##### Declaration

```cs
Critical = 128
```
#### LikelyCheater
Message value only possible with cheats/mods/etc

##### Declaration

```cs
LikelyCheater = 256
```
#### CustomError

Mirage errors will be defined from bits 0 to 16. the remaining 16 bits can be used for custom error.

CustomError can be used as start of bit shift. 
MyError1 = CustomError &lt;&lt; 0 
MyError2 = CustomError &lt;&lt; 1 



##### Declaration

```cs
CustomError = 65536
```
