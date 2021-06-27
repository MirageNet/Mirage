using System;
using Mirage.SocketLayer;

namespace Mirage
{
    /// <summary>
    /// Reason why Client was stopped or disconnected
    /// </summary>
    /// <remarks>
    /// Use different enums than SocketLayer so that:
    ///   User doesn't need to add reference to socket layer to use event;
    ///   Give high level reason so that they are easierto understand by the end user.
    /// </remarks>
    [Serializable]
    public enum ClientStoppedReason
    {
        /// <summary>No reason given</summary>
        None = 0,

        /// <summary>Connecting timed out
        /// <para>Server not sending replies</para></summary>
        Timeout = 1,
        /// <summary>Connection disconnect called locally</summary>
        LocalConnectionClosed = 2,
        /// <summary>Connection disconnect called on server</summary>
        RemoteConnectionClosed = 3,
        /// <summary>Server disconnected because sent packet was not allowed by server config</summary>
        InvalidPacket = 8,

        /// <summary>Server rejected connecting because it was full</summary>
        ServerFull = 4,
        /// <summary>Server did not reply</summary>
        ConnectingTimeout = 5,
        /// <summary>Disconnect called locally before server replies with connected</summary>
        ConnectingCancel = 6,

        /// <summary>Disconnect called when server was stopped in host mode</summary>
        HostModeStopped = 7,
    }


    internal static class StoppedReasonExtensions
    {
        public static ClientStoppedReason ToClientStoppedReason(this DisconnectReason reason)
        {
            switch (reason)
            {
                default:
                case DisconnectReason.None: return ClientStoppedReason.None;
                case DisconnectReason.Timeout: return ClientStoppedReason.Timeout;
                case DisconnectReason.RequestedByRemotePeer: return ClientStoppedReason.RemoteConnectionClosed;
                case DisconnectReason.RequestedByLocalPeer: return ClientStoppedReason.LocalConnectionClosed;
                case DisconnectReason.InvalidPacket: return ClientStoppedReason.InvalidPacket;
            }
        }

        public static ClientStoppedReason ToClientStoppedReason(this RejectReason reason)
        {
            switch (reason)
            {
                default:
                case RejectReason.None: return ClientStoppedReason.None;
                case RejectReason.Timeout: return ClientStoppedReason.ConnectingTimeout;
                case RejectReason.ServerFull: return ClientStoppedReason.ServerFull;
                case RejectReason.ClosedByPeer: return ClientStoppedReason.ConnectingCancel;
            }
        }
    }
}
