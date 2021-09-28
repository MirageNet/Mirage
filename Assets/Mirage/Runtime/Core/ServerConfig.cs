using System;
using Mirage.Events;
using Mirage.SocketLayer;

namespace Mirage.Core
{
    [Serializable]
    public class ServerConfig
    {
        //[Header("Components")]
        //[Tooltip("Creates Socket for Peer to use")]
        public SocketFactory SocketFactory;

        //[Tooltip("Authentication component attached to this object")]
        public NetworkAuthenticator Authenticator;

        /// <summary>
        /// The maximum number of concurrent network connections to support.
        /// <para>This field is only used if the <see cref="PeerConfig"/> property is null</para>
        /// </summary>
        //[Tooltip("Maximum number of concurrent connections.")]
        //[Min(1)]
        public int MaxConnections = 4;

        public bool DisconnectOnException = true;

        /// <summary>
        /// <para>If you disable this, the server will not listen for incoming connections on the regular network port.</para>
        /// <para>This can be used if the game is running in host mode and does not want external players to be able to connect - making it like a single-player game.</para>
        /// </summary>
        public bool Listening = true;

        //[Header("Debugging")]
        public bool EnablePeerMetrics;
        public int MetricsSize = 10;
    }
}
