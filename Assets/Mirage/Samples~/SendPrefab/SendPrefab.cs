using UnityEngine;

namespace Mirage.Examples
{
    public class SendPrefab : NetworkBehaviour
    {
        /// <summary>
        /// Array of prefabs show in the inspector
        /// <para>
        ///     Remember never trust the client, so an array like this can limit which prefabs the client can tell the server to spawn
        /// </para>
        /// </summary>
        public GameObject[] AllowedPrefab;


        private void Update()
        {
            // return if not the owner of this object
            if (!HasAuthority)
                return;

            // send Rpc to server when spacebar is pressed
            if (Input.GetKeyDown(KeyCode.Space))
            {
                // pick a random prefab, max is exclusive for int
                var index = Random.Range(0, AllowedPrefab.Length);

                // Send RPC to server telling it to spawn prefab
                CmdSpawnPrefab(index);
            }
        }


        // limit how often a client can spawn objects, so they can't flood the server
        [RateLimit(Interval = 1f, Refill = 2, MaxTokens = 5)]
        [ServerRpc]
        private void CmdSpawnPrefab(int index)
        {
            // Security: only a modified client could send an index outside the array, so kick them
            if (index < 0 || index >= AllowedPrefab.Length)
            {
                Owner.SetErrorAndDisconnect(PlayerErrorFlags.LikelyCheater);
                return;
            }

            // get the prefab from the index
            var prefab = AllowedPrefab[index];

            // get a position in front of this object.
            // calculated on the server, so the client can't spawn objects anywhere it wants
            var position = transform.position + (transform.forward * 2);

            // Instantiate and Spawn prefab
            var clone = Instantiate(prefab, position, Quaternion.identity);

            // spawn the new object and give authority to the player who called the RPC
            ServerObjectManager.Spawn(clone, Owner);
        }
    }
}
