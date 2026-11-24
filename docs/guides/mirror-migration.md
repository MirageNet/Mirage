---
sidebar_position: 10
---
# Mirror Migration Guide
This is a simple guide describing how to convert your Mirror project to Mirage.

## Namespace
First of all, `Mirror` namespace needs to be changed to `Mirage`. So in your code, replace all:

```cs
using Mirror;
```

with

```cs
using Mirage;
```

## Components
Many roles that `NetworkManager` fulfilled in Mirror were split into multiple simpler components in Mirage, such as `NetworkClient`, `NetworkServer`, and `NetworkSceneManager`. Those classes are no longer static singletons, they are MonoBehaviours instead, so you need to add them to your scene and reference them. `NetworkManager` in Mirage only serves as a reference holder for server and client.

:::tip
The easiest way to get started is to right-click in the Hierarchy > Network > NetworkManager. This will create a GameObject with all the necessary components and references already set up.
:::

### Accessing Mirage components from NetworkBehaviour
Despite Mirage removing all static states, you can still access the important networking components from within `NetworkBehaviour` easily. This table shows how to access different components in comparison to Mirror:

| Mirror (static) | Mirage (property of `NetworkBehaviour`) |
|:---------------:|:---------------------------------------:|
| `NetworkServer` | `Server`                                |
| `NetworkClient` | `Client`                                |
| `NetworkTime`   | `NetworkTime`                           |
| doesn't exist   | `ClientObjectManager`                   |
| doesn't exist   | `ServerObjectManager`                   |

## Network Events Lifecycle
Lifecycle management is no longer based on overrides. Instead, there are many UnityEvents that can be hooked into without direct coupling. They can also be used to hook callbacks via Unity Inspector.

:::tip
This guide only shows the Mirror counterpart events, but Mirage has more events available, so be sure to check them out as they might be useful.
:::

### Server and client events
The table below shows the override method names from Mirror's `NetworkManager` and the corresponding events from Mirage.

| Mirror (override)      | Mirage (event)                                                          |
|:----------------------:|:-----------------------------------------------------------------------:|
| `OnStartServer`        | [NetworkServer.Started](/docs/reference/Mirage/NetworkServer#started)              |
| `OnServerConnect`      | [NetworkServer.Authenticated](/docs/reference/Mirage/NetworkServer#authenticated)  |
| `OnServerDisconnect`   | [NetworkServer.Disconnected](/docs/reference/Mirage/NetworkServer#disconnected)    |
| `OnStopServer`         | [NetworkServer.Stopped](/docs/reference/Mirage/NetworkServer#stopped)              |
| `OnClientConnect`      | [NetworkClient.Authenticated](/docs/reference/Mirage/NetworkClient#authenticated)  |
| `OnClientDisconnect`   | [NetworkClient.Disconnected](/docs/reference/Mirage/NetworkClient#disconnected)    |


For example, this code from Mirror:

```cs
using Mirror;

public class MyNetworkManager : NetworkManager 
{
    public override void OnStartServer() 
    {
        // Server started
    }

    public override void OnServerConnect(NetworkConnection conn) 
    {
        // Client connected and authenticated on server
    }

    public override void OnStopServer() 
    {
        // Server stopped
    }

    public override void OnStartClient() 
    {
        // Client started
    }
        
    public override void OnClientConnect(NetworkConnection conn) 
    {
        // Client connected and authenticated
    }
            
    public override void OnClientDisconnect(NetworkConnection conn) 
    {
        // Client disconnected
    }
}
```

should be changed to:

```cs
using Mirage;

public class MyNetworkManager : NetworkManager 
{
    void Awake() 
    {
        Server.Started.AddListener(OnStartServer);
        Server.Authenticated.AddListener(OnServerConnect);
        Server.Stopped.AddListener(OnStopServer);
        Client.Started.AddListener(OnClientStarted);
        Client.Authenticated.AddListener(OnClientConnect);
        Client.Disconnected.AddListener(OnClientDisconnected);
    }

    void OnStartServer() 
    {
        // Server started
    }

    void OnServerConnect(INetworkPlayer conn) 
    {
        // Client connected (and authenticated) on server
    }

    void OnStopServer() 
    {
        // Server stopped
    }

    void OnClientStarted() 
    {
       // Client started
    }

    void OnClientConnect(INetworkPlayer conn) 
    {
        // Client connected
    }

    void OnClientDisconnected(ClientStoppedReason reason) 
    {
        // Client disconnected
    }
}
```

### NetworkBehaviour events

The table below shows the Mirror's `NetworkBehaviour` override method names on the left and the Mirage events on the right.

| Mirror (override)      | Mirage (event)                                                                |
|:----------------------:|:-----------------------------------------------------------------------------:|
| `OnStartServer`        | [Identity.OnStartServer](/docs/reference/Mirage/NetworkIdentity#onstartserver)           |
| `OnStopServer`         | [Identity.OnStopServer](/docs/reference/Mirage/NetworkIdentity#onstopserver)             |
| `OnStartClient`        | [Identity.OnStartClient](/docs/reference/Mirage/NetworkIdentity#onstartclient)           |
| `OnStopClient`         | [Identity.OnStopClient](/docs/reference/Mirage/NetworkIdentity#onstopclient)             |
| `OnStartLocalPlayer`   | [Identity.OnStartLocalPlayer](/docs/reference/Mirage/NetworkIdentity#onstartlocalplayer) |
| `OnStartAuthority`     | [Identity.OnAuthorityChanged](/docs/reference/Mirage/NetworkIdentity#onauthoritychanged) |
| `OnStopAuthority`      | [Identity.OnAuthorityChanged](/docs/reference/Mirage/NetworkIdentity#onauthoritychanged) |

Let's take this `Player` class as an example. In Mirror, you would do:

```cs
using Mirror;

public class Player : NetworkBehaviour 
{
    public override void OnStartServer() 
    {
        // Player started on server
    }

    public override void OnStartClient() 
    {
        // Player started on client
    }
}
```

Which should be changed like so in Mirage:

```cs
using Mirage;

public class Player : NetworkBehaviour 
{
    void Awake() 
    {
        Identity.OnStartServer.AddListener(OnStartServer);
        Identity.OnStartClient.AddListener(OnStartClient);
    }

    void OnStartServer() 
    {
        // Player started on server
    }

    void OnStartClient() 
    {
        // Player started on client
    }
}
```

:::note
Please note that due to timing all event callbacks should be registered in `Awake` method or via Unity inspector for them to be invoked consistently.
:::

## Method Attributes
The table below shows the new attribute names in Mirage.

| Mirror             | Mirage                                                                       |
|:------------------:|:----------------------------------------------------------------------------:|
| `[Command]`        | [[ServerRpc]](/docs/reference/Mirage/ServerRpcAttribute)                                |
| `[TargetRpc]`      | [[ClientRpc(target = Mirage.RpcTarget enum)]](/docs/reference/Mirage/ClientRpcAttribute) |
| `[ServerCallback]` | [[Server(error = false)]](/docs/reference/Mirage/ServerAttribute)                       |
| `[ClientCallback]` | [[Client(error = false)]](/docs/reference/Mirage/ClientAttribute)                       |
| doesn't exist      | [[HasAuthority(error = false)]](/docs/reference/Mirage/HasAuthorityAttribute)           |
| doesn't exist      | [[LocalPlayer(error = false)]](/docs/reference/Mirage/LocalPlayerAttribute)             |

## Renames
These fields/properties have been renamed:

| Mirror                                | Mirage                                                                                 |
|:-------------------------------------:|:--------------------------------------------------------------------------------------:|
| `ClientScene.localPlayer`             | [NetworkPlayer.Identity](/docs/reference/Mirage/NetworkPlayer#identity)                |
| `ClientScene.ready`                   | [Client.Player.SceneIsReady](/docs/reference/Mirage/NetworkPlayer#sceneisready)        |
| `NetworkIdentity.assetId`             | [NetworkIdentity.PrefabHash](/docs/reference/Mirage/NetworkIdentity#prefabhash)        |
| `NetworkIdentity.netId`               | [NetworkIdentity.NetId](/docs/reference/Mirage/NetworkIdentity#netid)                  |
| `NetworkIdentity.connectionToClient`  | [NetworkIdentity.Owner](/docs/reference/Mirage/NetworkIdentity#owner)                  |
| `NetworkBehaviour.isServer`           | [NetworkBehaviour.IsServer](/docs/reference/Mirage/NetworkBehaviour#isserver)          |
| `NetworkBehaviour.connectionToClient` | [NetworkBehaviour.Owner](/docs/reference/Mirage/NetworkBehaviour#owner)                |
| `NetworkBehaviour.connectionToServer` | Removed, use [Client.Player](/docs/reference/Mirage/NetworkClient#player) instead      |
| `NetworkBehaviour.hasAuthority`       | [NetworkBehaviour.HasAuthority](/docs/reference/Mirage/NetworkBehaviour#hasauthority)  |
| `NetworkBehaviour.Identity`           | [NetworkBehaviour.Identity](/docs/reference/Mirage/NetworkBehaviour#identity)          |
| `NetworkBehaviour.netId`              | [NetworkBehaviour.NetId](/docs/reference/Mirage/NetworkBehaviour#netid)                |
| `NetworkBehaviour.isClientOnly`       | [NetworkBehaviour.IsClientOnly](/docs/reference/Mirage/NetworkBehaviour#isclientonly)  |
| `NetworkBehaviour.islocalPlayer`      | [NetworkBehaviour.IsLocalPlayer](/docs/reference/Mirage/NetworkBehaviour#islocalplayer)|
| `NetworkConnection.isReady`           | [NetworkPlayer.SceneIsReady](/docs/reference/Mirage/NetworkPlayer#sceneisready)        |
| `NetworkConnection.identity`          | [NetworkPlayer.Identity](/docs/reference/Mirage/NetworkPlayer#identity)                |
| `NetworkServer.active`                | [NetworkServer.Active](/docs/reference/Mirage/NetworkServer#active)                    |
| `NetworkServer.localConnection`       | [NetworkServer.LocalPlayer](/docs/reference/Mirage/NetworkServer#localplayer)          |
| `NetworkClient.connection`            | [NetworkClient.Player](/docs/reference/Mirage/NetworkClient#player)                    |
| `NetworkTime.time`                    | [NetworkTime.Time](/docs/reference/Mirage/NetworkTime#time)                            |

## Object Management
Registered spawnable prefabs were moved from `NetworkManager` to the [ClientObjectManager](/docs/reference/Mirage/ClientObjectManager) component. You can use the Inspector to register all NetworkIdentities via a single click.

### Spawning and destroying
Table below shows how to spawn objects in Mirage from `NetworkBehaviour`:

| Mirror                  | Mirage                                                                                     |
|:-----------------------:|:------------------------------------------------------------------------------------------:|
| `NetworkServer.Spawn`   | [ServerObjectManager.Spawn](/docs/reference/Mirage/ServerObjectManager#spawnnetworkidentity) |
| `NetworkServer.Destroy` | [ServerObjectManager.Destroy](/docs/reference/Mirage/ServerObjectManager#destroygameobject-boolean)         |
