using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Mirror
{
    public class NetworkConnectionToClient : NetworkConnection
    {
        public NetworkConnectionToClient(IConnection connection) : base(connection)
        {
        }

        /// <summary>
        /// Disconnects this connection.
        /// </summary>
        public override void Disconnect()
        {
            // set not ready and handle clientscene disconnect in any case
            // (might be client or host mode here)
            isReady = false;
            RemoveObservers();
            Connection.Disconnect();
        }
    }
}
