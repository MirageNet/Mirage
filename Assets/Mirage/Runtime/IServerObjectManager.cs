using UnityEngine;

namespace Mirage
{
    public interface IServerObjectManager
    {
        void AddCharacter(INetworkPlayer player, GameObject character);
        void AddCharacter(INetworkPlayer player, GameObject character, int prefabHash);
        void AddCharacter(INetworkPlayer player, NetworkIdentity identity);

        void ReplaceCharacter(INetworkPlayer player, GameObject character, bool keepAuthority = false);
        void ReplaceCharacter(INetworkPlayer player, GameObject character, int prefabHash, bool keepAuthority = false);
        void ReplaceCharacter(INetworkPlayer player, NetworkIdentity identity, bool keepAuthority = false);

        void RemoveCharacter(INetworkPlayer player, bool keepAuthority = false);
        void DestroyCharacter(INetworkPlayer player, bool destroyServerObject = true);

        void Spawn(GameObject obj, INetworkPlayer owner = null);
        void Spawn(GameObject obj, GameObject ownerObject);
        void Spawn(GameObject obj, int prefabHash, INetworkPlayer owner = null);
        void Spawn(NetworkIdentity identity);
        void Spawn(NetworkIdentity identity, INetworkPlayer owner);

        void Destroy(GameObject obj, bool destroyServerObject = true);

        void SpawnObjects();
    }
}
