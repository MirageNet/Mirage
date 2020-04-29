using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Mirror
{
    public interface INetworkClient
    {
        void OnAuthenticated(INetworkConnection conn);

        void Disconnect();

        Task SendAsync<T>(T message, int channelId = Channels.DefaultReliable) where T : IMessageBase;

        void Send<T>(T message, int channelId = Channels.DefaultReliable) where T : IMessageBase;

        bool RemovePlayer();

        void Ready(INetworkConnection conn);

        void PrepareToSpawnSceneObjects();

        bool GetPrefab(Guid assetId, out GameObject prefab);

        void RegisterPrefab(GameObject prefab);

        void RegisterPrefab(GameObject prefab, Guid newAssetId);

        void RegisterPrefab(GameObject prefab, SpawnDelegate spawnHandler, UnSpawnDelegate unspawnHandler);

        void RegisterPrefab(GameObject prefab, SpawnHandlerDelegate spawnHandler, UnSpawnDelegate unspawnHandler);

        void UnregisterPrefab(GameObject prefab);

        void RegisterSpawnHandler(Guid assetId, SpawnDelegate spawnHandler, UnSpawnDelegate unspawnHandler);

        void RegisterSpawnHandler(Guid assetId, SpawnHandlerDelegate spawnHandler, UnSpawnDelegate unspawnHandler);

        void UnregisterSpawnHandler(Guid assetId);

        void ClearSpawners();

        void DestroyAllClientObjects();
    }
}
