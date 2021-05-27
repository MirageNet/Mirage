using System;

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

        /// <summary>Server rejected connecting because it was full</summary>
        ServerFull = 4,
        /// <summary>Server did not reply</summary>
        ConnectingTimeout = 5,
        /// <summary>Disconnect called locally before server replies with connected</summary>
        ConnectingCancel = 6,
    }
}
