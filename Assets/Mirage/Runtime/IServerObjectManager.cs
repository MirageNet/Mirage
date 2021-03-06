using System;
using UnityEngine;

namespace Mirage
{
    public interface IServerObjectManager
    {

        /// <summary>
        /// Raised when the client spawns an object
        /// </summary>
        SpawnEvent Spawned { get; }

        /// <summary>
        /// Raised when the client unspawns an object
        /// </summary>
        SpawnEvent UnSpawned { get; }

        bool AddCharacter(INetworkPlayer player, GameObject character);

        bool AddCharacter(INetworkPlayer player, GameObject character, Guid assetId);

        bool ReplaceCharacter(INetworkPlayer player, NetworkClient client, GameObject character, bool keepAuthority = false);

        bool ReplaceCharacter(INetworkPlayer player, NetworkClient client, GameObject character, Guid assetId, bool keepAuthority = false);

        void Spawn(GameObject obj, GameObject owner);

        void Spawn(GameObject obj, INetworkPlayer owner = null);

        void Spawn(GameObject obj, Guid assetId, INetworkPlayer owner = null);

        void Destroy(GameObject obj);

        void UnSpawn(GameObject obj);

        void SpawnObjects();
    }
}
