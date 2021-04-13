using System;
using UnityEngine;

namespace Mirage
{
    public interface IServerObjectManager
    {
        bool AddCharacter(INetworkPlayer player, GameObject character);

        bool AddCharacter(INetworkPlayer player, GameObject character, Guid assetId);

        bool ReplaceCharacter(INetworkPlayer player, GameObject character, bool keepAuthority = false);

        bool ReplaceCharacter(INetworkPlayer player, GameObject character, Guid assetId, bool keepAuthority = false);

        void Spawn(GameObject obj, GameObject owner);

        void Spawn(GameObject obj, INetworkPlayer owner = null);

        void Spawn(GameObject obj, Guid assetId, INetworkPlayer owner = null);

        void Destroy(GameObject obj);

        void UnSpawn(GameObject obj);

        void SpawnObjects();
    }
}
