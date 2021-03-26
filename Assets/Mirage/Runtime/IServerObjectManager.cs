using System;
using UnityEngine;

namespace Mirage
{
    public interface IServerObjectManager
    {
        bool AddCharacter(NetworkPlayer player, GameObject character);

        bool AddCharacter(NetworkPlayer player, GameObject character, Guid assetId);

        bool ReplaceCharacter(NetworkPlayer player, INetworkClient client, GameObject character, bool keepAuthority = false);

        bool ReplaceCharacter(NetworkPlayer player, INetworkClient client, GameObject character, Guid assetId, bool keepAuthority = false);

        void Spawn(GameObject obj, GameObject owner);

        void Spawn(GameObject obj, NetworkPlayer owner = null);

        void Spawn(GameObject obj, Guid assetId, NetworkPlayer owner = null);

        void Destroy(GameObject obj);

        void UnSpawn(GameObject obj);

        void SpawnObjects();
    }
}
