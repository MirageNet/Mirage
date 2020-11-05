using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Mirror
{
    // Handles requests to spawn objects on the client
    public delegate NetworkIdentity SpawnDelegate(Vector3 position, Guid assetId);

    public delegate NetworkIdentity SpawnHandlerDelegate(SpawnMessage msg);

    // Handles requests to unspawn objects on the client
    public delegate void UnSpawnDelegate(NetworkIdentity spawned);

    public interface IClientObjectManager
    {
        GameObject GetPrefab(Guid assetId);

        void RegisterPrefab(NetworkIdentity prefab);

        void RegisterPrefab(NetworkIdentity prefab, Guid newAssetId);

        void RegisterPrefab(NetworkIdentity prefab, SpawnDelegate spawnHandler, UnSpawnDelegate unspawnHandler);

        void RegisterPrefab(NetworkIdentity prefab, SpawnHandlerDelegate spawnHandler, UnSpawnDelegate unspawnHandler);

        void UnregisterPrefab(NetworkIdentity prefab);

        void RegisterSpawnHandler(Guid assetId, SpawnDelegate spawnHandler, UnSpawnDelegate unspawnHandler);

        void RegisterSpawnHandler(Guid assetId, SpawnHandlerDelegate spawnHandler, UnSpawnDelegate unspawnHandler);

        void UnregisterSpawnHandler(Guid assetId);

        void ClearSpawners();

        void DestroyAllClientObjects();

        void PrepareToSpawnSceneObjects();
    }

    public interface INetworkClient
    {
        void Disconnect();

        void Send<T>(T message, int channelId = Channel.Reliable);

        UniTask SendAsync<T>(T message, int channelId = Channel.Reliable);
    }
}
