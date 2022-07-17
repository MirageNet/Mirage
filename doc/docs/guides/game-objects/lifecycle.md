---
sidebar_position: 1
title: Lifecycle
---

# Lifecycle of a GameObject

Networked GameObjects go through several lifecycle states. 
You can add custom logic to the object lifecycle events by subscribing to the corresponding event in [NetworkIdentity](/docs/reference/Mirage/NetworkIdentity)

| Server                             | Client                                    |
| ---------------------------------- | ----------------------------------------- |
| [Instantiate](#server-instantiate) |                                           |
| [Start Server](#server-start)      |                                           |
|                                    | [Instantiate](#client-instantiate)        |
|                                    | [StartAuthority](#client-start-authority) |
|                                    | [StartClient](#start-client)              |
|                                    | [StartLocalPlayer](#start-local-player)   |
|                                    | [StopAuthority](#stop-authority)          |
| [StopServer](#server-stop)         |                                           |
| [Destroy](#server-destroy)         |                                           |
|                                    | [StopClient](#stop-client)                |
|                                    | [Destroy](#client-destroy)                |


:::note
In Mirror and UNet, you can add logic to lifecycle events by overriding methods in NetworkBehaviour.  
In Mirage you do it by subscribing to events in [NetworkIdentity](/docs/reference/Mirage/NetworkIdentity)
:::

## Server Instantiate

This is done usual by you using Unity's `GameObject.Instantiate` 
This goes through the regular GameObject Lifecycle events such as Awake, Start, Enabled, etc..
Basically this is outside Mirage's control.

[Scene Objects](/docs/guides/game-objects/scene-objects) are normally instantiated as part of the scene.

## Server Start

To start a server object, [spawn it](/docs/guides/game-objects/spawn-object). If you wish to perform some logic when the object starts in the server, add a component in your gameObject with our own method and subscribe to [NetworkIdentity.OnStartServer](/docs/reference/Mirage/NetworkIdentity#onstartserver)

For example:

```cs
public class MyComponent : MonoBehaviour {

    public void Awake() 
    {
        GetComponent<NetworkIdentity>.OnStartServer.AddListener(OnStartServer);
    }

    public void OnStartServer() 
    {
        Debug.Log("The object started on the server")
    }
}
```

You can also simply drag your `OnStartServer` method in the [NetworkIdentity.OnStartServer](/docs/reference/Mirage/NetworkIdentity#onstartserver) event in the inspector.

During the spawn a message will be sent to all the clients telling them to spawn the object. The message
will include all the data in [SyncVars](/docs/guides/sync/sync-var), [SyncLists](/docs/guides/sync/sync-list), [SyncHashSet](/docs/guides/sync/sync-hash-set), [SyncDictionary](/docs/guides/sync/sync-dictionary)

## Client Instantiate

When an object is spawned,  the server will send a message to the clients telling it to spawn a GameObject and provide 
an asset id.

By default, Mirage will look up all the known prefabs looking for that asset id.  
Make sure to add your prefabs in the NetworkClient list of prefabs.
Then Mirage will instantiate the prefab,  and it will go through the regular Unity Lifecycle events.
You can customize how objects are instantiated using Spawn Handlers.

Do not add Network logic to these events.  Instead,  use these events to subscribe to network events in NetworkIdentity.

Immediatelly after the object is instantiated, all the data is updated to match the data in the server.

## Client Start Authority

If the object is owned by this client, then NetworkIdentity will invoke the [NetworkIdentity.OnAuthorityChanged](/docs/reference/Mirage/NetworkIdentity#onauthoritychanged)
Subscribe to this event either by using `AddListener`,  or adding your method to the event in the inspector.
Note the Authority can be revoked, and granted again.  Every time the client gains authority, this event will be invoked again.

## Start Client

The event [NetworkIdentity.OnStartClient](/docs/reference/Mirage/NetworkIdentity#onstartclient) will be invoked. 
Subscribe to this event by using `AddListener` or adding your method in the event in the inspector

## Start Local Player

If the object spawned is the [character object](/docs/guides/game-objects/spawn-player),  the event [NetworkIdentity.OnStartLocalPlayer](/docs/reference/Mirage/NetworkIdentity#onstartlocalplayer)
is invoked.
Subscribe to this event by using `AddListener` or adding your method in the event in the inspector

## Stop Authority

If the object loses authority over the object, then NetworkIdentity will invoke the [NetworkIdentity.OnAuthorityChanged](/docs/reference/Mirage/NetworkIdentity#onauthoritychanged)
Subscribe to this event either by using `AddListener`,  or adding your method to the event in the inspector.
Note the Authority can be revoked, and granted again.  Every time the client loses authority, this event will be invoked again.

## Server Stop

Either because the client disconnected, the server stopped, 
you called [`ServerObjectManager.Destroy(GameObject, Boolean)`](/docs/reference/Mirage/ServerObjectManager#destroygameobject-boolean) the object may stop in the server.
During this state, a message is sent to all the clients to unspawn the object.
The event [NetworkIdentity.OnStopServer](/docs/reference/Mirage/NetworkIdentity#onstopserver) will be invoked. 

Subscribe to this event either by using `AddListener`,  or adding your method to the event in the inspector.

## Server Destroy

By default, the server will call `GameObject.Destroy` to destroy the object.  
Note that if it is a [Scene Object](/docs/guides/game-objects/scene-objects) the server will invoke `GameObject.SetActive(false)` instead.  

The regular unity lifecycle events apply.

Note that the server will destroy the object, and will not wait for the clients to unspawn their objects.

## Stop Client

This can be triggered either because the client received an Unspawn message or the client was disconnected
The event [NetworkIdentity.OnStopClient](/docs/reference/Mirage/NetworkIdentity#onstopclient) will be invoke.  
Subscribe to this event either by using `AddListener`,  or adding your method to the event in the inspector.

Use it to cleanup any network related resource used by this object.

## Client Destroy

After an object is stopped on the client,  by default unity will call `GameObject.Destroy` if it is a prefab [Spawned Object](/docs/guides/game-objects/spawn-object)
Or it will call `GameObject.SetActive(false)` if it is a [Scene Object](/docs/guides/game-objects/scene-objects)
You can customize how objects are destroying using Spawn Handlers

The normal Unity lifecycle events applies.
