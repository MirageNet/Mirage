using UnityEngine;

namespace Mirage.DisplayMetrics
{
    public class SetDisplayMetrics : MonoBehaviour
    {
        public NetworkServer server;
        public NetworkClient client;
        public DisplayMetricsAverageGui displayMetrics;

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
            // dont set metrics if client is host (clients metrics will be null)
            if (client.IsHost)
                return;

            displayMetrics.Metrics = client.Metrics;
        }
    }
}
