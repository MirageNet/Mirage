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
                // pick a random prefab
                var index = Random.Range(0, AllowedPrefab.Length - 1);

                // get a position in from of the current object
                var position = transform.position + (transform.forward * 2);

                // Send RPC to server telling it to spawn prefab
                RpcSpawnPrefab(index, position);
            }
        }


        [ServerRpc]
        private void RpcSpawnPrefab(int index, Vector3 position)
        {
            // get the prefab from the index
            // Security Note: you may want to validate the index here,
            //                otherwise IndexOutOfRangeException might be throw if invalid index is sent
            //                if DisconnectOnException is true then Mirage will automatically kick the client if an Exception is throw
            var prefab = AllowedPrefab[index];

            // Instantiate and Spawn prefab
            var clone = Instantiate(prefab, position, Quaternion.identity);

            // get the owner of this object
            var owner = Owner;

            // spawn the new object and give authority to the player who called the RPC
            ServerObjectManager.Spawn(clone, owner);
        }
    }
}
