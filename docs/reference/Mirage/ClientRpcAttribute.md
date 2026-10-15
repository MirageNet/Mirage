---
id: ClientRpcAttribute
title: ClientRpcAttribute
---

# Class ClientRpcAttribute


The server uses a Remote Procedure Call (RPC) to run this function on specific clients.
Note that if you set the target as Connection, you need to pass a specific connection as a parameter of your method



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
public class ClientRpcAttribute : Attribute, _Attribute
```


### Fields

#### channel

##### Declaration

```cs
public Channel channel
```
#### target

##### Declaration

```cs
public RpcTarget target
```
#### excludeOwner

##### Declaration

```cs
public bool excludeOwner
```
#### excludeHost
stops method being called on host/server

##### Declaration

```cs
public bool excludeHost
```
