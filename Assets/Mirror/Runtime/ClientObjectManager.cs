using UnityEngine;

namespace Mirror
{
    /// <summary>
    /// This is a client object manager class used by the networking system. It manages spawning and unspawning of objects for the network client.
    /// </summary>
    [RequireComponent(typeof(NetworkClient))]
    [DisallowMultipleComponent]
    public class ClientObjectManager : MonoBehaviour
    {
        static readonly ILogger logger = LogFactory.GetLogger(typeof(NetworkClient));

        public NetworkClient client;

        private void Start()
        {
            if (logger.LogEnabled()) logger.Log("ClientObjectManager started");

            client.Connected.AddListener(Connected);
        }

        void Connected(INetworkConnection conn)
        {
            if (client.IsLocalClient)
            {
                RegisterHostHandlers(conn);
            }
            else
            {
                RegisterMessageHandlers(conn);
            }
        }

        internal void RegisterHostHandlers(INetworkConnection connection)
        {

        }

        internal void RegisterMessageHandlers(INetworkConnection connection)
        {

        }
    }
}
