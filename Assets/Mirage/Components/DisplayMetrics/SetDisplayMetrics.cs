using Mirage.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Mirage.DisplayMetrics
{
    public class SetDisplayMetrics : MonoBehaviour
    {
        [FormerlySerializedAs("server")]
        public NetworkServer NetworkServer;
        public NetworkClient client;
        public DisplayMetricsAverageGui displayMetrics;

        private Server server => NetworkServer.Server;

        private void Start()
        {
            if (server != null)
                server.Started.AddListener(ServerStarted);
            if (client != null)
                client.Connected.AddListener(ClientConnected);
        }

        private void ServerStarted()
        {
            displayMetrics.Metrics = server.Metrics;
        }

        private void ClientConnected(INetworkPlayer arg0)
        {
            displayMetrics.Metrics = client.Metrics;
        }
    }
}
