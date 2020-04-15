using UnityEngine;

namespace Mirror
{

    public class ClientObjectManager : MonoBehaviour
    {
        public NetworkClient client;

        private void Start()
        {
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
