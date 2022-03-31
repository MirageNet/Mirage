using System;
using System.Collections;
using System.Collections.Generic;
using Mirage.Logging;
using UnityEngine;

namespace Mirage.Authenticators
{
    /// <summary>
    /// An authenticator that disconnects connections if they don't
    /// authenticate within a specified time limit.
    /// </summary>
    [AddComponentMenu("Network/Authenticators/TimeoutAuthenticator")]
    public class TimeoutAuthenticator : NetworkAuthenticator
    {
        static readonly ILogger logger = LogFactory.GetLogger(typeof(TimeoutAuthenticator));

        public NetworkAuthenticator Authenticator;

        [Range(0, 600), Tooltip("Timeout to auto-disconnect in seconds. Set to 0 for no timeout.")]
        public float Timeout = 60;

        public void Awake()
        {
            Authenticator.OnClientAuthenticated += HandleClientAuthenticated;
            Authenticator.OnServerAuthenticated += HandleServerAuthenticated;
        }

        private readonly HashSet<INetworkPlayer> pendingAuthentication = new HashSet<INetworkPlayer>();

        private void HandleServerAuthenticated(INetworkPlayer player)
        {
            pendingAuthentication.Remove(player);
            ServerAccept(player);
        }

        private void HandleClientAuthenticated(INetworkPlayer player)
        {
            pendingAuthentication.Remove(player);
            ClientAccept(player);
        }

        public override void ServerAuthenticate(INetworkPlayer player)
        {
            pendingAuthentication.Add(player);
            Authenticator.ServerAuthenticate(player);
            if (Timeout > 0)
                StartCoroutine(BeginAuthentication(player, ServerReject));
        }

        public override void ClientAuthenticate(INetworkPlayer player)
        {
            pendingAuthentication.Add(player);
            Authenticator.ClientAuthenticate(player);

            if (Timeout > 0)
                StartCoroutine(BeginAuthentication(player, ClientReject));
        }

        public override void ServerSetup(NetworkServer server)
        {
            Authenticator.ServerSetup(server);
        }

        public override void ClientSetup(NetworkClient client)
        {
            Authenticator.ClientSetup(client);
        }

        IEnumerator BeginAuthentication(INetworkPlayer player, Action<INetworkPlayer> reject)
        {
            if (logger.LogEnabled()) logger.Log($"Authentication countdown started for {player}: {Timeout} seconds.");

            yield return new WaitForSecondsRealtime(Timeout);

            if (pendingAuthentication.Contains(player))
            {
                if (logger.LogEnabled()) logger.Log($"Authentication timed out for {player}. Disconnecting client.");

                pendingAuthentication.Remove(player);
                reject.Invoke(player);
            }
        }
    }
}
