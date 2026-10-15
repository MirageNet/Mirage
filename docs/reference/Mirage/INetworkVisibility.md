---
id: INetworkVisibility
title: INetworkVisibility
---

# Interface INetworkVisibility




##### Syntax

```cs
public interface INetworkVisibility
```

### Methods
#### OnCheckObserver(INetworkPlayer)



##### Declaration

```cs
bool OnCheckObserver(INetworkPlayer player)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.INetworkPlayer | player |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean |  |

#### OnRebuildObservers(HashSet&lt;INetworkPlayer&gt;, Boolean)



##### Declaration

```cs
void OnRebuildObservers(HashSet<INetworkPlayer> observers, bool initialize)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Collections.Generic.HashSet&lt;Mirage.INetworkPlayer&gt; | observers |  |
| System.Boolean | initialize |  |


