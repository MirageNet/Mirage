using System;
using UnityEngine;

namespace Mirage
{
    [Flags]
    public enum NetworkManagerMode
    {
        None = 0,
        Server = 1,
        Client = 2,
        Host = Server | Client
    }

    [AddComponentMenu("Network/NetworkManager")]
    [HelpURL("https://miragenet.github.io/Mirage/docs/guides/callbacks/network-manager")]
    [DisallowMultipleComponent]
    public class NetworkManager : MonoBehaviour
    {
        public NetworkServer Server;
        public NetworkClient Client;
        public NetworkSceneManager NetworkSceneManager;
        public ServerObjectManager ServerObjectManager;
        public ClientObjectManager ClientObjectManager;

        /// <summary>
        /// True if the server or client is started and running
        /// <para>This is set True in StartServer / StartClient, and set False in StopServer / StopClient</para>
        /// </summary>
        public bool IsNetworkActive => Server.Active || Client.Active;

        /// <summary>
        /// helper enum to know if we started the networkmanager as server/client/host.
        /// </summary>
        public NetworkManagerMode NetworkMode
        {
            get
            {
                if (!Server.Active && !Client.Active)
                    return NetworkManagerMode.None;
                else if (Server.Active && Client.Active)
                    return NetworkManagerMode.Host;
                else if (Server.Active)
                    return NetworkManagerMode.Server;
                else
                    return NetworkManagerMode.Client;
            }
        }
    }
}
