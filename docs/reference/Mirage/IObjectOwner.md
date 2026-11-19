---
id: IObjectOwner
title: IObjectOwner
---

# Interface IObjectOwner


An object that can own networked objects




##### Syntax

```cs
public interface IObjectOwner
```


### Properties

#### Identity

The main object owned by this player, normally the player&apos;s character


##### Declaration

```cs
NetworkIdentity Identity { get; set; }
```
#### HasCharacter

##### Declaration

```cs
bool HasCharacter { get; }
```
#### OwnedObjects

All the objects owned by the player


##### Declaration

```cs
IReadOnlyCollection<NetworkIdentity> OwnedObjects { get; }
```
### Methods
#### AddOwnedObject(NetworkIdentity)



##### Declaration

```cs
void AddOwnedObject(NetworkIdentity networkIdentity)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.NetworkIdentity | networkIdentity |  |


#### RemoveOwnedObject(NetworkIdentity)



##### Declaration

```cs
void RemoveOwnedObject(NetworkIdentity networkIdentity)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.NetworkIdentity | networkIdentity |  |


#### RemoveAllOwnedObject(Boolean)


Removes all owned objects. This is useful to call when player disconnects to avoid objects being destroyed



##### Declaration

```cs
void RemoveAllOwnedObject(bool sendAuthorityChangeEvent)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Boolean | sendAuthorityChangeEvent | Should message be send to owner client? If player is disconnecting you should set this false |


#### DestroyOwnedObjects()


Destroys or unspawns all owned objects.
This is called when the player is disconnects.
It will be called after , so Disconnected can be used to remove any owned objects from the list before they are destroyed.



##### Declaration

```cs
void DestroyOwnedObjects()
```


