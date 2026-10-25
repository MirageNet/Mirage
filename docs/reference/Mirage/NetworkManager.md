---
id: NetworkManager
title: NetworkManager
---

# Class NetworkManager



<div class="inheritance">

##### Inheritance

<div class="level" style={{"--data-index": 0}}>
System.Object
</div>
</div>

##### Syntax

```cs
public class NetworkManager : MonoBehaviour
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
#### ValidateReferences

##### Declaration

```cs
public bool ValidateReferences
```

### Properties

#### IsNetworkActive

True if the server or client is started and running
This is set True in StartServer / StartClient, and set False in StopServer / StopClient


##### Declaration

```cs
public bool IsNetworkActive { get; }
```
#### NetworkMode

helper enum to know if we started the NetworkManager as server/client/host.


##### Declaration

```cs
public NetworkManagerMode NetworkMode { get; }
```
