using System.Collections.Generic;
using UnityEngine;

namespace Mirage.Snippets.Spawning
{
    // CodeEmbed-Start: dynamic-spawning
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
    // CodeEmbed-End: dynamic-spawning
}
