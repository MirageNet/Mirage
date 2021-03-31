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

        // This will get more code in the near future
        internal void OnServerAuthenticateInternal(INetworkPlayer player)
        {
            OnServerAuthenticate(player);
        }

        /// <summary>
        /// Called on server from OnServerAuthenticateInternal when a client needs to authenticate
        /// </summary>
        /// <param name="player">Connection to client.</param>
        public virtual void OnServerAuthenticate(INetworkPlayer player)
        {
            OnServerAuthenticated?.Invoke(player);
        }

        #endregion

        #region client

        // This will get more code in the near future
        internal void OnClientAuthenticateInternal(INetworkPlayer player)
        {
            OnClientAuthenticate(player);
        }

        /// <summary>
        /// Called on client from OnClientAuthenticateInternal when a client needs to authenticate
        /// </summary>
        /// <param name="player">Connection of the client.</param>
        public virtual void OnClientAuthenticate(INetworkPlayer player)
        {
            OnClientAuthenticated?.Invoke(player);
        }

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
