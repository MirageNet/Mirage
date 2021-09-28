using Mirage.Core;
using Mirage.SocketLayer;
using UnityEngine;

namespace Mirage
{
    /// <summary>
    /// The NetworkServer.
    /// </summary>
    [AddComponentMenu("Network/NetworkServer")]
    [DisallowMultipleComponent]
    public class NetworkServer : MonoBehaviour
    {
        public ServerConfig Config = new ServerConfig();
        public ServerEvents Events = new ServerEvents();

        public Server Server { get; private set; }

        internal void Awake()
        {
            if (Config.SocketFactory == null) Config.SocketFactory = GetComponent<SocketFactory>();

            Server = new Server(Config, Events);
        }

        internal void Update()
        {
            Server.Update();
        }
    }
}
