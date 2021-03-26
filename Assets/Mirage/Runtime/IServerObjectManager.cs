using System;
using UnityEngine;

namespace Mirage
{
    public interface IServerObjectManager
    {
        bool AddCharacter(IObjectOwner player, GameObject character);

        bool AddCharacter(IObjectOwner player, GameObject character, Guid assetId);

        bool ReplaceCharacter(IObjectOwner player, INetworkClient client, GameObject character, bool keepAuthority = false);

        bool ReplaceCharacter(IObjectOwner player, INetworkClient client, GameObject character, Guid assetId, bool keepAuthority = false);

        void Spawn(GameObject obj, GameObject owner);

        void Spawn(GameObject obj, IObjectOwner owner = null);

        void Spawn(GameObject obj, Guid assetId, IObjectOwner owner = null);

        void Destroy(GameObject obj);

        void UnSpawn(GameObject obj);

        void SpawnObjects();
    }
}
