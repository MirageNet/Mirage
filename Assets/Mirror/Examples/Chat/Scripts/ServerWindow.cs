using UnityEngine;

namespace Mirror.Examples.Chat
{

    public class ServerWindow : MonoBehaviour
    {
        public string serverIp = "localhost";

        public NetworkHost networkHost;

        public void StartClient()
        {
            //TODO: networkHost.LocalClient.ConnectAsync(serverIp);
        }

        public void StartHost()
        {
            _ = networkHost.StartHost();
        }

        public void SetServerIp(string serverIp)
        {
            this.serverIp = serverIp;
        }
    }
}
