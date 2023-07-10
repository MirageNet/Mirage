using System.Collections.Generic;
using UnityEngine;

namespace Mirage.Snippets.Spawning
{
    // CodeEmbed-Start: dynamic-spawning
    public class DynamicSpawning : MonoBehaviour
    {
        public ServerObjectManager ServerObjectManager;
        public ClientObjectManager ClientObjectManager;

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
                // send index as prefabHash
                ServerObjectManager.Spawn(_preSpawnedObjects[i], prefabHash: i);
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
            // prefabHash starts with 16 bits of 0, then it an id we are using for spawning
            // this chance of this happening randomly is very low    
            // you can do more validation on the hash based on use case
            return (prefabHash & 0xFFFF) == 0;
        }

        // finds object based on hash and returns it
        public NetworkIdentity FindPreSpawnedObject(SpawnMessage spawnMessage)
        {
            var prefabHash = spawnMessage.PrefabHash.Value;
            // we stored index in last 16 bits on hash
            var index = prefabHash >> 16;

            var identity = _preSpawnedObjects[index];
            return identity;
        }
    }
    // CodeEmbed-End: dynamic-spawning
}
