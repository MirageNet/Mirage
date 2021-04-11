using System;
using UnityEngine;

namespace Mirage
{
    public interface IServerObjectManager
    {
        void AddCharacter(INetworkPlayer player, GameObject character);

        void AddCharacter(INetworkPlayer player, GameObject character, Guid assetId);

        void ReplaceCharacter(INetworkPlayer player, INetworkClient client, GameObject character, bool keepAuthority = false);

        void ReplaceCharacter(INetworkPlayer player, INetworkClient client, GameObject character, Guid assetId, bool keepAuthority = false);

        void Spawn(GameObject obj, GameObject owner);

        void Spawn(GameObject obj, INetworkPlayer owner = null);

        void Spawn(GameObject obj, Guid assetId, INetworkPlayer owner = null);

        void Destroy(GameObject obj);

        void UnSpawn(GameObject obj);

        void SpawnObjects();
    }
}
