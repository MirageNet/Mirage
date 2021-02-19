using System;
using UnityEngine;
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
        /// <summary>
        /// Raised when the client spawns an object
        /// </summary>
        SpawnEvent Spawned { get; }

        /// <summary>
        /// Raised when the client unspawns an object
        /// </summary>
        SpawnEvent UnSpawned { get; }

        GameObject GetPrefab(Guid assetId);

        void RegisterPrefab(NetworkIdentity prefab);

        void RegisterPrefab(NetworkIdentity prefab, Guid newAssetId);

        void RegisterPrefab(NetworkIdentity prefab, SpawnHandlerDelegate spawnHandler, UnSpawnDelegate unspawnHandler);

        void UnregisterPrefab(NetworkIdentity prefab);

        void RegisterSpawnHandler(Guid assetId, SpawnHandlerDelegate spawnHandler, UnSpawnDelegate unspawnHandler);

        void UnregisterSpawnHandler(Guid assetId);

        void ClearSpawners();

        void DestroyAllClientObjects();

        void PrepareToSpawnSceneObjects();
    }
}
