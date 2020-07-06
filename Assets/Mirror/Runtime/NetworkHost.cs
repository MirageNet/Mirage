using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Mirror
{
    [AddComponentMenu("Network/NetworkHost")]
    [RequireComponent(typeof(NetworkClient))]
    [DisallowMultipleComponent]
    public class NetworkHost : NetworkServer
    {
        static readonly ILogger logger = LogFactory.GetLogger<NetworkHost>();

        NetworkClient client;

        /// <summary>
        /// This is invoked when a host is started.
        /// <para>StartHost has multiple signatures, but they all cause this hook to be called.</para>
        /// </summary>
        public UnityEvent OnStartHost = new UnityEvent();

        /// <summary>
        /// This is called when a host is stopped.
        /// </summary>
        public UnityEvent OnStopHost = new UnityEvent();

        /// <summary>
        /// True if the server or client is started and running
        /// <para>This is set True in StartServer / StartClient, and set False in StopServer / StopClient</para>
        /// </summary>
        public bool IsNetworkActive => Active || LocalClient.Active;

        /// <summary>
        /// This starts a network "host" - a server and client in the same application.
        /// </summary>
        public async Task StartHost()
        {
            await ListenAsync();

            SpawnObjects();

            client.ConnectHost(this);

            OnStartHost.Invoke();

            // server scene was loaded. now spawn all the objects
            SpawnObjects();

            ActivateHostScene();
        }

        /// <summary>
        /// This stops both the client and the server that the manager is using.
        /// </summary>
        public void StopHost()
        {
            OnStopHost.Invoke();
            LocalClient.Disconnect();
            Disconnect();
        }
    }
}
