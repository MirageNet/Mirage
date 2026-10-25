---
id: NetworkSceneLoader
title: NetworkSceneLoader
---

# Class NetworkSceneLoader


NetworkSceneLoader handles simple scene loading and spawning the player character.
For more complex use cases, it it best to create a copy of this script and modify it for your needs.



<div class="inheritance">

##### Inheritance

<div class="level" style={{"--data-index": 0}}>
System.Object
</div>
</div>

##### Syntax

```cs
public class NetworkSceneLoader : MonoBehaviour
```


### Fields

#### Server

##### Declaration

```cs
public NetworkServer Server
```
#### Client

##### Declaration

```cs
public NetworkClient Client
```
#### ServerObjectManager

##### Declaration

```cs
public ServerObjectManager ServerObjectManager
```
#### ClientObjectManager

##### Declaration

```cs
public ClientObjectManager ClientObjectManager
```
#### PlayerPrefab

##### Declaration

```cs
public NetworkIdentity PlayerPrefab
```
#### TargetScene

##### Declaration

```cs
public string TargetScene
```
#### ServerLoading

##### Declaration

```cs
public bool ServerLoading
```
### Methods
#### ServerLoadScene(String)


Starts a server-authoritative scene load.



##### Declaration

```cs
public UniTask ServerLoadScene(string scenePath)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.String | scenePath |  |

##### Returns
| Type | Description |
| ---- | ---- |
| UniTask |  |

