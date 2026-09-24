---
id: GameObjectSerializers
title: GameObjectSerializers
---

# Class GameObjectSerializers



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
public static class GameObjectSerializers
```

### Methods
#### WriteGameObjectSyncVar(NetworkWriter, GameObjectSyncvar)



##### Declaration

```cs
public static void WriteGameObjectSyncVar(this NetworkWriter writer, GameObjectSyncvar id)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| Mirage.GameObjectSyncvar | id |  |


#### ReadGameObjectSyncVar(NetworkReader)



##### Declaration

```cs
public static GameObjectSyncvar ReadGameObjectSyncVar(this NetworkReader reader)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.GameObjectSyncvar |  |

