---
sidebar_position: 1
title: Network Behaviour
---
# Network Behaviour Callbacks

**See also [NetworkBehaviour](/docs/reference/Mirage/NetworkBehaviour) in the API Reference.**

There are a number of events relating to network behaviours that can occur over the course of a normal multiplayer game. These include events such as the host starting up, a player joining, or a player leaving. Each of these possible events has an associated callback that you can implement in your own code to take action when the event occurs.

To use an event you must add a function as a listener, this function will then be called when the event occurs. Some events, like `OnStartServer`, will call the listener immediately if the event was previously called. This allows you to add the listeners at any point without worrying about missing the Invoke.

```cs
void Awake()
{
    Identity.OnStartServer.AddListener(MyStartServer);
    Identity.OnStartClient.AddListener(MyStartClient);
    Identity.OnStartLocalPlayer.AddListener(MyStartLocalPlayer);
}

void MyStartServer() 
{
    // ...
}

void MyStartClient() 
{
    // ...
}

void MyStartLocalPlayer() 
{
    // ...
}
```

This is a full list of virtual methods (callbacks) that you can implement on `NetworkBehaviour`, and where they are called

## Server Only

- OnStartServer
    - called when behaviour is spawned on the server
- OnStopServer
    - called when behaviour is destroyed or unspawned on the server
- OnSerialize
    - called when behaviour is serializing before it is sent to a client, when overriding make sure to call `base.OnSerialize`

## Client only

- OnStartClient
    - called when behaviour is spawned on a client 
- OnStartAuthority
    - called when behaviour has authority when it is spawned (eg local player)
    - called when behaviour is given authority by the sever
- OnStartLocalPlayer
    - called when the behaviour is on the local character object

- OnStopAuthority
    - called when authority is taken from the object (eg local player is replaced but not destroyed)
- OnStopClient
    - called when an object is destroyed on a client by the `ObjectDestroyMessage` or `ObjectHideMessage` messages


# Example flows 

Below is some example call order for different modes

:::note
`Start` is called by Unity before the first frame, while normally this happens after Mirage's callbacks. But if you don't call `NetworkServer.Spawn` the same frame as `Instantiate` then start may be called first
:::

### Server mode

When `NetworkServer.Spawn` is called (eg when new client connections and a player is created)
-   `OnStartServer`
-   `OnRebuildObservers`
-   `Start`

### Client mode

When the local player is spawned for the client
-   `OnStartAuthority`
-   `OnStartClient`
-   `OnStartLocalPlayer`
-   `Start`

### Host mode

These are only called on the **Player Game Objects** when a client connects:
-   `OnStartServer`
-   `OnRebuildObservers`
-   `OnStartAuthority`
-   `OnStartClient`
-   `OnStartLocalPlayer`
-   `Start` 
