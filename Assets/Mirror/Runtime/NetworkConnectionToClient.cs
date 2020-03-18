using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mirror
{
    public class NetworkConnectionToClient : NetworkConnection
    {
        public NetworkConnectionToClient(int networkConnectionId) : base(networkConnectionId)
        {
        }

        public override string address => Transport.activeTransport.ServerGetClientAddress(connectionId);

        // internal because no one except Mirror should send bytes directly to
        // the client. they would be detected as a message. send messages instead.
        readonly List<int> singleConnectionId = new List<int> { -1 };

        protected override bool Send(ArraySegment<byte> segment, int channelId = Channels.DefaultReliable)
        {
            if (logNetworkMessages) Debug.Log("ConnectionSend " + this + " bytes:" + BitConverter.ToString(segment.Array, segment.Offset, segment.Count));

            singleConnectionId[0] = connectionId;
            return Transport.activeTransport.ServerSend(singleConnectionId, channelId, segment);
        }

        /// <summary>
        /// Disconnects this connection.
        /// </summary>
        public override void Disconnect()
        {
            // set not ready and handle clientscene disconnect in any case
            // (might be client or host mode here)
            isReady = false;
            Transport.activeTransport.ServerDisconnect(connectionId);
            RemoveObservers();
        }
    }
}
