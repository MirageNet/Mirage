using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Mirror
{
    public class NetworkConnectionToServer : NetworkConnection
    {

        public NetworkConnectionToServer(IConnection connection): base(connection)
        {

        }

        /// <summary>
        /// Disconnects this connection.
        /// </summary>
        public override void Disconnect()
        {
            // set not ready and handle clientscene disconnect in any case
            // (might be client or host mode here)
            ClientScene.HandleClientDisconnect(this);
            base.Disconnect();
        }
    }
}
