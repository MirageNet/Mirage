using UnityEngine;

namespace Mirage.Snippets.UsingSyncPrefab
{
    // CodeEmbed-Start: shoot
    public class Shooter : NetworkBehaviour
    {
        // add [NetworkedPrefab] to ensure prefab is network object in inspector
        [NetworkedPrefab]
        public GameObject Prefab;

        // call this on server
        public void Shoot(Vector3 position, Quaternion rotation)
        {
            // spawn prefab locally
            var clone = Instantiate(Prefab, position, rotation);

            // then send to clients so they can also spawn locally

            RpcShoot(new SyncPrefab(Prefab.GetNetworkIdentity()), position, rotation);
        }

        [ClientRpc]
        public void RpcShoot(SyncPrefab syncPrefab, Vector3 position, Quaternion rotation)
        {
            // find prefab from objectManager
            var prefab = syncPrefab.FindPrefab(ClientObjectManager);

            // spawn prefab locally
            var clone = Instantiate(prefab, position, rotation);
        }
    }
    // CodeEmbed-End: shoot
}
