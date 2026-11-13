---
id: RecentlyDestroyed
title: RecentlyDestroyed
---

# Class RecentlyDestroyed



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
public class RecentlyDestroyed
```

### Methods
#### Add(UInt32)



##### Declaration

```cs
public void Add(uint netId)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.UInt32 | netId |  |


#### WasRecentlyDestroyed(UInt32, out Double)



##### Declaration

```cs
public bool WasRecentlyDestroyed(uint netId, out double destroyTime)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.UInt32 | netId |  |
| System.Double | destroyTime |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean |  |

#### CleanUp(Double)



##### Declaration

```cs
public void CleanUp(double gracePeriod)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Double | gracePeriod |  |


