using System;
using UnityEngine.Events;

namespace Mirage
{
    public delegate NetworkIdentity SpawnHandlerDelegate(SpawnMessage msg);

    // Handles requests to unspawn objects on the client
    public delegate void UnSpawnDelegate(NetworkIdentity spawned);

    [Serializable]
    public class SpawnEvent : UnityEvent<NetworkIdentity> { }

    public interface IClientObjectManager
    {
        NetworkIdentity GetPrefab(int prefabHash);

        void RegisterPrefab(NetworkIdentity identity);

        void RegisterPrefab(NetworkIdentity identity, int newPrefabHash);

        void RegisterPrefab(NetworkIdentity identity, SpawnHandlerDelegate spawnHandler, UnSpawnDelegate unspawnHandler);

        void UnregisterPrefab(NetworkIdentity identity);

        void RegisterSpawnHandler(int prefabHash, SpawnHandlerDelegate spawnHandler, UnSpawnDelegate unspawnHandler);

        void UnregisterSpawnHandler(int prefabHash);

        void ClearSpawners();

        void DestroyAllClientObjects();

        void PrepareToSpawnSceneObjects();
    }
}
