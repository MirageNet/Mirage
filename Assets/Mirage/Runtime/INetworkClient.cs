using Mirage.Events;

namespace Mirage
{
    public interface INetworkClient : IMessageSender
    {

        /// <summary>
        /// Event fires once the Client has connected its Server.
        /// </summary>
        IAddLateEvent<INetworkPlayer> Connected { get; }

        /// <summary>
        /// Event fires after the Client connection has successfully been authenticated with its Server.
        /// </summary>
        IAddLateEvent<INetworkPlayer> Authenticated { get; }

        /// <summary>
        /// Event fires after the Client has disconnected from its Server and Cleanup has been called.
        /// </summary>
        IAddLateEvent<ClientStoppedReason> Disconnected { get; }

        /// <summary>
        /// The NetworkConnection object this client is using.
        /// </summary>
        INetworkPlayer Player { get; }

        /// <summary>
        /// active is true while a client is connecting/connected
        /// (= while the network is active)
        /// </summary>
        bool Active { get; }

        /// <summary>
        /// NetworkClient can connect to local server in host mode too
        /// </summary>
        bool IsLocalClient { get; }

        NetworkWorld World { get; }

        MessageHandler MessageHandler { get; }

        void Disconnect();
    }
}
