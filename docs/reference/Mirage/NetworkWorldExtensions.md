---
id: NetworkWorldExtensions
title: NetworkWorldExtensions
---

# Class NetworkWorldExtensions



<div class="inheritance">

##### Inheritance

<div class="level" style={{"--data-index": 0}}>
System.Object
</div>
</div>

##### Inherited Members

<details>
<summary>Show</summary>
</details>

##### Syntax

```cs
public static class NetworkWorldExtensions
```

### Methods
#### AddAndInvokeOnSpawn(NetworkWorld, Action&lt;NetworkIdentity&gt;)


adds an event handler, and invokes it on current objects in world



##### Declaration

```cs
public static void AddAndInvokeOnSpawn(this NetworkWorld world, Action<NetworkIdentity> action)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.NetworkWorld | world |  |
| System.Action&lt;Mirage.NetworkIdentity&gt; | action |  |


#### AddAndInvokeOnAuthorityChanged(NetworkWorld, AuthorityChanged)


adds an event handler, and invokes it on current objects in world



##### Declaration

```cs
public static void AddAndInvokeOnAuthorityChanged(this NetworkWorld world, AuthorityChanged action)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.NetworkWorld | world |  |
| Mirage.AuthorityChanged | action |  |


