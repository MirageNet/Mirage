---
id: IVisibilityTracker
title: IVisibilityTracker
---

# Interface IVisibilityTracker


An object that can observe NetworkIdentities.
this is useful for interest management




##### Syntax

```cs
public interface IVisibilityTracker
```


### Properties

#### VisList

HashSet of all  that this player can see
Only valid on server
Reverse collection for 


##### Declaration

```cs
IReadOnlyCollection<NetworkIdentity> VisList { get; }
```
### Methods
#### AddToVisList(NetworkIdentity)


Called when sending spawn message to client



##### Declaration

```cs
void AddToVisList(NetworkIdentity identity)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.NetworkIdentity | identity |  |


#### RemoveFromVisList(NetworkIdentity)


Called when sending destroy message to client



##### Declaration

```cs
void RemoveFromVisList(NetworkIdentity identity)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.NetworkIdentity | identity |  |


#### RemoveAllVisibleObjects()


Removes all  that this player can see
This is called when loading a new scene



##### Declaration

```cs
void RemoveAllVisibleObjects()
```


#### ContainsInVisList(NetworkIdentity)


Checks if player can see 



##### Declaration

```cs
bool ContainsInVisList(NetworkIdentity identity)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.NetworkIdentity | identity |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean |  |

