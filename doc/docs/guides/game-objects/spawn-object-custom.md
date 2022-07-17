---
sidebar_position: 6
title: Spawn Object - Custom
---
# Custom Spawn Functions

You can use spawn handler functions to customize the default behavior when creating spawned game objects on the client. Spawn handler functions ensure you have full control of how you spawn the game object, as well as how you destroy it.

Use `ClientObjectManager.RegisterSpawnHandler` or `ClientObjectManager.RegisterPrefab` to register functions to spawn and destroy client game objects. The server creates game objects directly, and then spawns them on the clients through this functionality. This functions takes either the asset ID or a prefab and two function delegates: one to handle creating game objects on the client, and one to handle destroying game objects on the client. The asset ID can be a dynamic one, or just the asset ID found on the prefab game object you want to spawn.

The spawn / unspawn delegates will look something like this:

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

When a prefab is saved it's `PrefabHash` field will be automatically set. If you want to create prefabs at runtime you will have to generate a new Hash instead.

**Generate prefab at runtime**
``` cs
// Create a hash that can be generated on both server and client
// using a string and GetStableHashCode is a good way to do this
int coinHash = "MyCoin".GetStableHashCode();

// register handlers using hash
ClientObjectManager.RegisterSpawnHandler(creatureHash, SpawnCoin, UnSpawnCoin);
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

Here is an example of how you might set up a simple game object pooling system with custom spawn handlers. Spawning and unspawning then puts game objects in or out of the pool.

``` cs
using System.Collections.Generic;
using Mirage;
using UnityEngine;

namespace Mirage.Examples
{
    public class PrefabPoolManager : MonoBehaviour
    {
        [Header("Settings")]
        public ClientObjectManager clientObjectManager;
        public int startSize = 5;
        public int maxSize = 20;
        public NetworkIdentity prefab;

        [Header("Debug")]
        [SerializeField] int currentCount;

        Queue<NetworkIdentity> pool;

        void Start()
        {
            InitializePool();

            clientObjectManager.RegisterPrefab(prefab, SpawnHandler, UnspawnHandler);
        }

        // used by clientObjectManager.RegisterPrefab
        NetworkIdentity SpawnHandler(SpawnMessage msg)
        {
            return GetFromPool(msg.position, msg.rotation);
        }

        // used by clientObjectManager.RegisterPrefab
        void UnspawnHandler(NetworkIdentity spawned)
        {
            PutBackInPool(spawned);
        }

        void OnDestroy()
        {
            clientObjectManager.UnregisterPrefab(prefab);
        }

        private void InitializePool()
        {
            pool = new Queue<NetworkIdentity>();
            for (int i = 0; i < startSize; i++)
            {
                NetworkIdentity next = CreateNew();

                pool.Enqueue(next);
            }
        }

        NetworkIdentity CreateNew()
        {
            if (currentCount > maxSize)
            {
                Debug.LogError($"Pool has reached max size of {maxSize}");
                return null;
            }

            // use this object as parent so that objects dont crowd hierarchy
            NetworkIdentity next = Instantiate(prefab, transform);
            next.name = $"{prefab.name}_pooled_{currentCount}";
            next.gameObject.SetActive(false);
            currentCount++;
            return next;
        }

        /// <summary>
        /// Used to take Object from Pool.
        /// <para>Should be used on server to get the next Object</para>
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <returns></returns>
        public NetworkIdentity GetFromPool(Vector3 position, Quaternion rotation)
        {
            NetworkIdentity next = pool.Count > 0
                ? pool.Dequeue() // take from pool
                : CreateNew(); // create new because pool is empty

            // CreateNew might return null if max size is reached
            if (next == null) { return null; }

            // set position/rotation and set active
            next.transform.position = position;
            next.transform.rotation = rotation;
            next.gameObject.SetActive(true);
            return next;
        }

        /// <summary>
        /// Used to put object back into pool so they can b
        /// <para>Should be used on server after unspawning an object</para>
        /// </summary>
        /// <param name="spawned"></param>
        public void PutBackInPool(NetworkIdentity spawned)
        {
            // disable object
            spawned.gameObject.SetActive(false);

            // add back to pool
            pool.Enqueue(spawned);
        }
    }
}
```

To use this manager, create a new empty game object and add the `PrefabPoolManager` component (code above). Next, drag a prefab you want to spawn multiple times to the Prefab field, and set `startSize` and `maxSize` fields. `startSize` is how many will be spawned when your game starts. `maxSize` is the max number that can be spawned, if this number is reached then an error will be given when trying to more new objects.

Finally, set up a reference to the PrefabPoolManager in the script you are using for player movement:

``` cs
PrefabPoolManager prefabPoolManager;

void Start()
{
    prefabPoolManager = FindObjectOfType<PrefabPoolManager>();
}
```

Your player logic might contain something like this, which moves and fires coins:

``` cs
void Update()
{
    if (!isLocalPlayer)
        return;
    
    // move
    var x = Input.GetAxis("Horizontal") * 0.1f;
    var z = Input.GetAxis("Vertical") * 0.1f;
    transform.Translate(x, 0, z);

    // shoot
    if (Input.GetKeyDown(KeyCode.Space))
    {
        // Server RPC Call function is called on the client, but invoked on the server
        CmdFire();
    }
}
```

In the fire logic on the player, make it use the game object pool:

``` cs
[ServerRpc]
void CmdFire()
{
    // Set up bullet on server
    NetworkIdentity bullet = prefabPoolManager.GetFromPool(transform.position + transform.forward, Quaternion.identity);

    Rigidbody rigidBody = bullet.GetComponent<Rigidbody>();
    rigidBody.velocity = transform.forward * 4;

    // tell server to send SpawnMessage, which will call SpawnHandler on client
    ServerObjectManager.Spawn(bullet);

    // destroy bullet after 2 seconds
    StartCoroutine(DestroyDelay(bullet, 2.0f));
}

IEnumerator DestroyDelay(NetworkIdentity go, float delay)
{
    yield return new WaitForSeconds(delay);

    // return object to pool on server
    prefabPoolManager.PutBackInPool(go);

    // tell server to send ObjectDestroyMessage, which will call UnspawnHandler on client
    ServerObjectManager.Destroy(go, destroyServerObject: false);
}
```

The Destroy method above shows how to return game objects to the pool so that they can be re-used when you fire again
