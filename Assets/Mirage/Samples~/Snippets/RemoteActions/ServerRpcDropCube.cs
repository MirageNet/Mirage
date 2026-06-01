using UnityEngine;
using Mirage;

namespace Mirage.Snippets.RemoteActions.ServerRpcDropCube
{
    // Dummy class to make the snippet code compile
    public static class NetworkServer
    {
        public static void Spawn(GameObject obj) {}
    }

    // CodeEmbed-Start: server-rpc-drop-cube
    public class Player : NetworkBehaviour
    {
        // Assigned in inspector
        public GameObject cubePrefab;

        private void Update()
        {
            if (!IsLocalPlayer)
                return;

            if (Input.GetKey(KeyCode.X))
                DropCube();
        }

        [ServerRpc]
        private void DropCube()
        {
            if (cubePrefab != null)
            {
                Vector3 spawnPos = transform.position + transform.forward * 2;
                Quaternion spawnRot = transform.rotation;
                GameObject cube = Instantiate(cubePrefab, spawnPos, spawnRot);
                NetworkServer.Spawn(cube);
            }
        }
    }
    // CodeEmbed-End: server-rpc-drop-cube
}
