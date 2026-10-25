---
id: ServerRpcAttribute
title: ServerRpcAttribute
---

# Class ServerRpcAttribute


Call this from a client to run this function on the server.
Make sure to validate input etc. It&apos;s not possible to call this from a server.



<div class="inheritance">

##### Inheritance

<div class="level" style={{"--data-index": 0}}>
System.Object
</div>
<div class="level" style={{"--data-index": 1}}>
System.Attribute
</div>
</div>

##### Inherited Members

<details>
<summary>Show</summary>
</details>

##### Syntax

```cs
[AttributeUsage(AttributeTargets.Method)]
public class ServerRpcAttribute : Attribute, _Attribute
```


### Fields

#### channel

##### Declaration

```cs
public Channel channel
```
#### requireAuthority

##### Declaration

```cs
public bool requireAuthority
```
#### allowServerToCall

Allows the server to invoke the method locally. Note: this will bypass any authority checks on host.


##### Declaration

```cs
public bool allowServerToCall
```
