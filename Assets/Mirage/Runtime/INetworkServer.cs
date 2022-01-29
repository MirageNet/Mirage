using System.Collections.Generic;
using Mirage.Events;

namespace Mirage
{
    public interface INetworkServer
    {
        /// <summary>
        /// This is invoked when a server is started - including when a host is started.
        /// </summary>
        IAddLateEvent Started { get; }

        /// <summary>
        /// Event fires once a new Client has connect to the Server.
        /// </summary>
        NetworkPlayerEvent Connected { get; }

        /// <summary>
        /// Event fires once a new Client has passed Authentication to the Server.
        /// </summary>
        NetworkPlayerEvent Authenticated { get; }

        /// <summary>
        /// Event fires once a Client has Disconnected from the Server.
        /// </summary>
        NetworkPlayerEvent Disconnected { get; }

        IAddLateEvent Stopped { get; }

        /// <summary>
        /// This is invoked when a host is started.
        /// <para>StartHost has multiple signatures, but they all cause this hook to be called.</para>
        /// </summary>
        IAddLateEvent OnStartHost { get; }

        /// <summary>
        /// This is called when a host is stopped.
        /// </summary>
        IAddLateEvent OnStopHost { get; }

        /// <summary>
        /// The connection to the host mode client (if any).
        /// </summary>
        INetworkPlayer LocalPlayer { get; }

        /// <summary>
        /// The host client for this server 
        /// </summary> 
        INetworkClient LocalClient { get; }

        /// <summary>
        /// True if there is a local client connected to this server (host mode)
        /// </summary>
        bool LocalClientActive { get; }

        /// <summary>
        /// <para>Checks if the server has been started.</para>
        /// <para>This will be true after NetworkServer.Listen() has been called.</para>
        /// </summary>
        bool Active { get; }

        NetworkWorld World { get; }

        SyncVarSender SyncVarSender { get; }

        MessageHandler MessageHandler { get; }

        IReadOnlyCollection<INetworkPlayer> Players { get; }

        void Stop();

        void AddConnection(INetworkPlayer player);

        void RemoveConnection(INetworkPlayer player);

        void SendToAll<T>(T msg, int channelId = Channel.Reliable);
    }
}
