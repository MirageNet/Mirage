---
sidebar_position: 6
title: Spawn Object - Custom
---
# Custom Spawn Functions

You can use spawn handler functions to customize the default behavior when creating spawned game objects on the client. Spawn handler functions ensure you have full control of how you spawn the game object, as well as how you destroy it.

Use `ClientObjectManager.RegisterSpawnHandler` or `ClientObjectManager.RegisterPrefab` to register functions to spawn and destroy client game objects. The server creates game objects directly and then spawns them on the clients through this functionality. This function takes either the asset ID or a prefab and two function delegates: one to handle creating game objects on the client, and one to handle destroying game objects on the client. The asset ID can be a dynamic one, or just the asset ID found on the prefab game object you want to spawn.

The spawn/unspawn delegates will look something like this:

**Spawn Handler**
```cs
NetworkIdentity SpawnDelegate(SpawnMessage msg) 
 {
     // do stuff here
     return null;
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
```cs
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
 ServerObjectManager.Spawn(gameObject, coinHash);
```

The spawn functions themselves are implemented with the delegate signature. Here is the coin spawner. The `SpawnCoin` would look the same, but have different spawn logic:

```cs
public NetworkIdentity SpawnCoin(SpawnMessage msg)
 {
     return Instantiate(m_CoinPrefab, msg.SpawnValues.Position ?? m_CoinPrefab.transform.position, msg.SpawnValues.Rotation ?? m_CoinPrefab.transform.rotation);
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
     return GetFromPool(msg.SpawnValues.Position ?? prefab.transform.position, msg.SpawnValues.Rotation ?? prefab.transform.rotation);
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

```cs
public class DynamicSpawning : MonoBehaviour
 {
     public ServerObjectManager ServerObjectManager;
     public ClientObjectManager ClientObjectManager;

     // used to check if a prefabHash is for a pre-spawned object
     // we use a high bit to avoid collisions with other hashes
     private const int PRE_SPAWN_HASH_BIT = 1 << 30;

     // store handler in field so that you dont need to allocate a new one for each DynamicSpawn call
     private SpawnHandler _handler;
     private List<NetworkIdentity> _preSpawnedObjects = new List<NetworkIdentity>();

     // call this on server to spawn objects and send spawn message to client
     public void SpawnOnServer()
     {
         // set up local objects
         SpawnLocal();

         // send spawn message
         for (var i = 0; i < _preSpawnedObjects.Count; i++)
         {
             var prefabHash = PRE_SPAWN_HASH_BIT | i;
             // send index as prefabHash
             ServerObjectManager.Spawn(_preSpawnedObjects[i], prefabHash: prefabHash);
         }
     }

     // call this on client to spawn object and set up handler to receive spawn message 
     public void SpawnOnClient()
     {
         // set up local objects
         SpawnLocal();

         // register handler so client can find objects when server sends spawn message
         _handler = new SpawnHandler(FindPreSpawnedObject, null);
         ClientObjectManager.RegisterDynamicSpawnHandler(DynamicSpawn);
     }

     private void SpawnLocal()
     {
         // fill _preSpawnedObjects here with objects
         // these can be prefabs or other objects you want to find
         _preSpawnedObjects.Add(new GameObject("object 1").AddComponent<NetworkIdentity>());
         _preSpawnedObjects.Add(new GameObject("object 2").AddComponent<NetworkIdentity>());
     }

     private SpawnHandler DynamicSpawn(int prefabHash)
     {
         // this will run for all SpawnMessages, so we must first check if this prefabHash is one we want to handle
         if (IsPreSpawnedId(prefabHash))
             // return a handler that is using FindPreSpawnedObject
             return _handler;
         else
             return null;
     }

     private bool IsPreSpawnedId(int prefabHash)
     {
         // check if high bit is set (if this is not set, it is for sure not part of our list)
         if ((prefabHash & PRE_SPAWN_HASH_BIT) == 0)
             return false;

         // if high bit is set, then the rest of the bits are the index
         var index = prefabHash & ~PRE_SPAWN_HASH_BIT;

         // NOTE: prefabHash could still randomly be from prefab at this point, check its against the _preSpawnedObjects count
         //       it is highly unlikely that a random hash will be small and under _preSpawnedObjects
         // check if index is in range of our pre-spawned list
         return index < _preSpawnedObjects.Count;
     }

     // finds object based on hash and returns it
     public NetworkIdentity FindPreSpawnedObject(SpawnMessage spawnMessage)
     {
         var prefabHash = spawnMessage.PrefabHash.Value;
         // remove high bit to get index
         var index = prefabHash & ~PRE_SPAWN_HASH_BIT;

         var identity = _preSpawnedObjects[index];
         return identity;
     }
 }
```