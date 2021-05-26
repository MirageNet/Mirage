using System;
using Mirage.SocketLayer;
using UnityEngine.Events;

namespace Mirage.Events
{
    [Serializable] public class DisconnectEvent : UnityEvent<ClientStoppedReason> { }

    /// <summary>
    /// Event fires from a <see cref="NetworkClient">NetworkClient</see> when it fails to connect to the server
    /// </summary>
    [Serializable] public class DisconnectAddLateEvent : AddLateEvent<ClientStoppedReason, DisconnectEvent> { }

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
