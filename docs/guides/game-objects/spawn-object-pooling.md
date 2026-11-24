---
sidebar_position: 7
title: Spawn Object - Pooling
---

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

In the firing logic on the player, make it use the game object pool:

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
