using UnityEngine;

namespace Mirage.Snippets.GameObjects
{
    // CodeEmbed-Start: network-world-events
    public class NetworkWorldEvents : MonoBehaviour
    {
        public NetworkServer Server;
        public NetworkClient Client;

        public void Awake()
        {
            // Client/Server.World is only set after server is started, 
            // so wait for start, then add event listener to OnSpawn
            Server.Started.AddListener(ServerStarted);
            Client.Started.AddListener(ClientStarted);
        }

        private void ServerStarted()
        {
            Server.World.onSpawn += OnServerSpawn;
            Server.World.onUnspawn += OnServerUnspawn;
        }

        private void OnServerSpawn(NetworkIdentity identity)
        {
            Debug.Log($"The object {identity} was spawned on the server");
        }

        private void OnServerUnspawn(uint netId, NetworkIdentity identity)
        {
            Debug.Log($"The object {identity} (netId={netId}) was unspawned on the server");
        }

        private void ClientStarted()
        {
            Client.World.onSpawn += OnClientSpawn;
            Client.World.onUnspawn += OnClientUnspawn;
        }

        private void OnClientSpawn(NetworkIdentity identity)
        {
            Debug.Log($"The object {identity} was spawned on the client");
        }

        private void OnClientUnspawn(uint netId, NetworkIdentity identity)
        {
            Debug.Log($"The object {identity} (netId={netId}) was unspawned on the client");
        }
    }
    // CodeEmbed-End: network-world-events
}
