---
id: SyncPrefabSerialize
title: SyncPrefabSerialize
---

# Class SyncPrefabSerialize



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
public static class SyncPrefabSerialize
```

### Methods
#### WriteSyncPrefab(NetworkWriter, SyncPrefab)



##### Declaration

```cs
public static void WriteSyncPrefab(this NetworkWriter writer, SyncPrefab value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| Mirage.SyncPrefab | value |  |


#### ReadSyncPrefab(NetworkReader)



##### Declaration

```cs
public static SyncPrefab ReadSyncPrefab(this NetworkReader reader)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.SyncPrefab |  |

