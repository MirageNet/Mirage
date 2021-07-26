using Mirage;
using UnityEngine;

namespace JamesFrowen.NetworkingBenchmark
{
    public class SetDisplayMetrics : MonoBehaviour
    {
        public NetworkServer server;
        public NetworkClient client;
        public DisplayMetrics_AverageGui displayMetrics;

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
