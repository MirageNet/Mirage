using UnityEngine.Events;

namespace Mirage
{
    public interface INetworkServer
    {
        /// <summary>
        /// This is invoked when a server is started - including when a host is started.
        /// </summary>
        UnityEvent Started { get; }

        /// <summary>
        /// Event fires once a new Client has connect to the Server.
        /// </summary>
        NetworkConnectionEvent Connected { get; }

        /// <summary>
        /// Event fires once a new Client has passed Authentication to the Server.
        /// </summary>
        NetworkConnectionEvent Authenticated { get; }

        /// <summary>
        /// Event fires once a Client has Disconnected from the Server.
        /// </summary>
        NetworkConnectionEvent Disconnected { get; }

        UnityEvent Stopped { get; }

        /// <summary>
        /// This is invoked when a host is started.
        /// <para>StartHost has multiple signatures, but they all cause this hook to be called.</para>
        /// </summary>
        UnityEvent OnStartHost { get; }

        /// <summary>
        /// This is called when a host is stopped.
        /// </summary>
        UnityEvent OnStopHost { get; }

        INetworkConnection LocalConnection { get; }

        // The host client for this server 
        NetworkClient LocalClient { get; }

        bool LocalClientActive { get; }

        bool Active { get; }

        void Disconnect();

        void AddConnection(INetworkConnection conn);

        void RemoveConnection(INetworkConnection conn);

        void SendToAll<T>(T msg, int channelId = Channel.Reliable);
    }
}
