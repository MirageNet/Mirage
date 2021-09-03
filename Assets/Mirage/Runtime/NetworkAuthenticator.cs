using System;
using UnityEngine;

namespace Mirage
{
    /// <summary>
    /// Base class for implementing component-based authentication during the Connect phase
    /// </summary>
    [HelpURL("https://miragenet.github.io/Mirage/Articles/Components/Authenticators/index.html")]
    public abstract class NetworkAuthenticator : MonoBehaviour
    {
        /// <summary>
        /// Notify subscribers on the server when a client is authenticated
        /// </summary>
        public event Action<INetworkPlayer> OnServerAuthenticated;

        /// <summary>
        /// Notify subscribers on the client when the client is authenticated
        /// </summary>
        public event Action<INetworkPlayer> OnClientAuthenticated;

        #region server

        /// <summary>
        /// Call this when player has been accepted on the server
        /// </summary>
        /// <param name="player"></param>
        protected void ServerAccept(INetworkPlayer player)
        {
            player.IsAuthenticated = true;
            OnServerAuthenticated?.Invoke(player);
        }
        /// <summary>
        /// Call this when player has been rejected on the server. This will disconnect the player.
        /// </summary>
        /// <param name="player"></param>
        protected void ServerReject(INetworkPlayer player)
        {
            player.Disconnect();
        }

        /// <summary>
        /// Used to set up authenticator on server
        /// <para>Can be used to register message handlers before any players connect</para>
        /// </summary>
        public abstract void ServerSetup(NetworkServer server);

        /// <summary>
        /// Authenticate the player on the Server.
        /// <para>Called by the server when new client connects</para>
        /// </summary>
        /// <param name="player"></param>
        public abstract void ServerAuthenticate(INetworkPlayer player);

        #endregion

        #region client
        /// <summary>
        /// Call this when player has been accepted on the client.
        /// </summary>
        /// <param name="player"></param>
        protected void ClientAccept(INetworkPlayer player)
        {
            player.IsAuthenticated = true;
            OnClientAuthenticated?.Invoke(player);
        }
        /// <summary>
        /// Call this when player has been rejected on the client. This will disconnect the player.
        /// </summary>
        /// <param name="player"></param>
        protected void ClientReject(INetworkPlayer player)
        {
            player.Disconnect();
        }

        /// <summary>
        /// Used to set up authenticator on client
        /// <para>Can be used to register message handlers before any player connects</para>
        /// </summary>
        public abstract void ClientSetup(NetworkClient client);

        /// <summary>
        /// Authenticate the player on the Client.
        /// <para>Called by the client after connected to the server</para>
        /// </summary>
        /// <param name="player"></param>
        public abstract void ClientAuthenticate(INetworkPlayer player);

        #endregion

#if UNITY_EDITOR
        void OnValidate()
        {
            UnityEditor.Undo.RecordObject(this, "Assigned NetworkClient authenticator");
            // automatically assign NetworkClient field if we add this to NetworkClient
            NetworkClient client = GetComponent<NetworkClient>();
            if (client != null && client.authenticator == null)
            {
                client.authenticator = this;
            }

            NetworkServer server = GetComponent<NetworkServer>();
            if (server != null && server.authenticator == null)
            {
                server.authenticator = this;
            }
        }
#endif
    }
}
