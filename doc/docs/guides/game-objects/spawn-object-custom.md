---
sidebar_position: 6
title: Spawn Object - Custom
---
# Custom Spawn Functions

You can use spawn handler functions to customize the default behavior when creating spawned game objects on the client. Spawn handler functions ensure you have full control of how you spawn the game object, as well as how you destroy it.

Use `ClientObjectManager.RegisterSpawnHandler` or `ClientObjectManager.RegisterPrefab` to register functions to spawn and destroy client game objects. The server creates game objects directly and then spawns them on the clients through this functionality. This function takes either the asset ID or a prefab and two function delegates: one to handle creating game objects on the client, and one to handle destroying game objects on the client. The asset ID can be a dynamic one, or just the asset ID found on the prefab game object you want to spawn.

The spawn/unspawn delegates will look something like this:

**Spawn Handler**
``` cs
NetworkIdentity SpawnDelegate(SpawnMessage msg) 
{
    // do stuff here
}
```

**UnSpawn Handler**
```cs
void UnSpawnDelegate(NetworkIdentity spawned) 
{
    // do stuff here
}
```

When a prefab is saved its `PrefabHash` field will be automatically set. If you want to create prefabs at runtime you will have to generate a new Hash instead.

**Generate prefab at runtime**
``` cs
// Create a hash that can be generated on both server and client
// using a string and GetStableHashCode is a good way to do this
int coinHash = "MyCoin".GetStableHashCode();

// register handlers using hash
ClientObjectManager.RegisterSpawnHandler(coinHash, SpawnCoin, UnSpawnCoin);
```

:::note
The unspawn function may be left as `null`, Mirage will then call `GameObject.Destroy` when the destroy message is received.
:::

**Use existing prefab**
```cs
// register handlers using prefab
ClientObjectManager.RegisterPrefab(coin, SpawnCoin, UnSpawnCoin);
```

**Spawn on Server**
```cs
int coinHash = "MyCoin".GetStableHashCode();

// spawn a coin - SpawnCoin is called on client
// pass in coinHash so that it is set on the Identity before it is sent to client
NetworkServer.Spawn(gameObject, coinHash);
```

The spawn functions themselves are implemented with the delegate signature. Here is the coin spawner. The `SpawnCoin` would look the same, but have different spawn logic:

``` cs
public NetworkIdentity SpawnCoin(SpawnMessage msg)
{
    return Instantiate(m_CoinPrefab, msg.position, msg.rotation);
}
public void UnSpawnCoin(NetworkIdentity spawned)
{
    Destroy(spawned);
}
```

When using custom spawn functions, it is sometimes useful to be able to unspawn game objects without destroying them. This can be done by calling `NetworkServer.Destroy(identity, destroyServerObject: false)`, making sure that the 2nd argument is false. This causes the object to be `Reset` on the server and sends a `ObjectDestroyMessage` to clients. The `ObjectDestroyMessage` will cause the custom unspawn function to be called on the clients. If there is no unspawn function the object will instead be `Destroy`

Note that on the host, game objects are not spawned for the local client, because they already exist on the server. This also means that no spawn or unspawn handler functions are called.

## Setting Up a Game Object Pool with Custom Spawn Handlers

you can use custom spawn handlers in order set up object pooling so you dont need to instantiate and destroy objects each time you use them. 

A full guide on pooling can be found here: [Spawn Object Pooling](./spawn-object-pooling)

```cs
void ClientConnected() 
{
    clientObjectManager.RegisterPrefab(prefab, PoolSpawnHandler, PoolUnspawnHandler);
}

// used by clientObjectManager.RegisterPrefab
NetworkIdentity PoolSpawnHandler(SpawnMessage msg)
{
    return GetFromPool(msg.position, msg.rotation);
}

// used by clientObjectManager.RegisterPrefab
void PoolUnspawnHandler(NetworkIdentity spawned)
{
    PutBackInPool(spawned);
}
```

## Dynamic spawning 

Some times you may want to create objects at runtime and you might not know the prefab hash ahead of time. For this you can use Dynamic Spawn Handlers to return a spawn handler for a prefab hash.

Below is an example where client pre-spawns objects while loading, and then network spawns them when receiving a `SpawnMessage` from server.

Dynamic Handler avoid the need to add 1 spawn handler for each prefab hash. Instead you can just add a single dynamic handler that can then be used to find and return objects.

{{{ Path:'Snippets/DynamicSpawning.cs' Name:'dynamic-spawning' }}}