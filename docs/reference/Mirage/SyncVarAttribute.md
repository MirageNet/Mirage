---
id: SyncVarAttribute
title: SyncVarAttribute
---

# Class SyncVarAttribute


SyncVars are used to synchronize a variable from the server to all clients automatically.
Value must be changed on server, not directly by clients.  Hook parameter allows you to define a client-side method to be invoked when the client gets an update from the server.



<div class="inheritance">

##### Inheritance

<div class="level" style={{"--data-index": 0}}>
System.Object
</div>
</div>

##### Syntax

```cs
[AttributeUsage(AttributeTargets.Field)]
public class SyncVarAttribute : PropertyAttribute
```


### Fields

#### hook
A function that should be called on the client when the value changes.

##### Declaration

```cs
public string hook
```
#### initialOnly

If true, this syncvar will only be sent with spawn message, any other changes will not be sent to existing objects


##### Declaration

```cs
public bool initialOnly
```
#### invokeHookOnServer

If true this syncvar hook will also fire on the server side.


##### Declaration

```cs
public bool invokeHookOnServer
```
#### invokeHookOnOwner

If true this syncvar hook will also fire the owner when it is sending data


##### Declaration

```cs
public bool invokeHookOnOwner
```
#### hookType

What type of look Mirage should look for


##### Declaration

```cs
public SyncHookType hookType
```
