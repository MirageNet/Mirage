<!-- todo -->

# NetworkBehaviour Callbacks

**See also <xref:Mirage.NetworkBehaviour> in the API Reference.**

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
    - called when behaviour is spawned on server
- OnStopServer
    - called when behaviour is destroyed or unspawned on server
- OnSerialize
    - called when behaviour is serialize before it is sent to client, when overriding make sure to call `base.OnSerialize`

## Client only

- OnStartClient
    - called when behaviour is spawned on client 
- OnStartAuthority
    - called when behaviour has authority when it is spawned (eg local player)
    - called when behaviour is given authority by the sever
- OnStartLocalPlayer
    - called when the behaviour is on the local character object

- OnStopAuthority
    - called when authority is taken from the object (eg local player is replaced but not destroyed)
- OnStopClient
    - called when object is destroyed on client by the `ObjectDestroyMessage` or `ObjectHideMessage` messages


# Example flows 

Below is some example call order for different modes

> NOTE: `Start` is called by unity before the first frame, while normally this happens after Mirage's callbacks. But if you dont call ` NetworkServer.Spawn` the same frame as `instantiate` then start may be called first

> Note: `OnRebuildObservers` and `OnSetHostVisibility` is now on `NetworkVisibility` instead of `NetworkBehaviour`

## Server mode

When a NetworkServer.Spawn is called (eg when new client connections and a player is created)
-   `OnStartServer`
-   `OnRebuildObservers`
-   `Start`

## Client mode

When local player is spawned for client
-   `OnStartAuthority`
-   `OnStartClient`
-   `OnStartLocalPlayer`
-   `Start`

## Host mode

These are only called on the **Player Game Objects** when a client connects:
-   `OnStartServer`
-   `OnRebuildObservers`
-   `OnStartAuthority`
-   `OnStartClient`
-   `OnSetHostVisibility`
-   `OnStartLocalPlayer`
-   `Start` 
