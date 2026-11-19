---
id: GameObjectExtensions
title: GameObjectExtensions
---

# Class GameObjectExtensions



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
public static class GameObjectExtensions
```

### Methods
#### GetNetworkIdentity(GameObject)


Gets  on a  and throws  if the GameObject does not have one.



##### Declaration

```cs
public static NetworkIdentity GetNetworkIdentity(this GameObject gameObject)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| GameObject | gameObject |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.NetworkIdentity | attached NetworkIdentity |

