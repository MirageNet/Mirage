using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirage.Logging;

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
            base.OnClientAuthenticate(player);
        }

        private void HandleClientAuthenticated(INetworkPlayer player)
        {
            pendingAuthentication.Remove(player);
            base.OnServerAuthenticate(player);
        }

        public override void OnClientAuthenticate(INetworkPlayer player)
        {
            pendingAuthentication.Add(player);
            Authenticator.OnClientAuthenticate(player);

            if (Timeout > 0)
                StartCoroutine(BeginAuthentication(player));
        }

        public override void OnServerAuthenticate(INetworkPlayer player)
        {
            pendingAuthentication.Add(player);
            Authenticator.OnServerAuthenticate(player);
            if (Timeout > 0)
                StartCoroutine(BeginAuthentication(player));
        }

        IEnumerator BeginAuthentication(INetworkPlayer player)
        {
            if (logger.LogEnabled()) logger.Log($"Authentication countdown started {player} {Timeout}");

            yield return new WaitForSecondsRealtime(Timeout);

            if (pendingAuthentication.Contains(player))
            {
                if (logger.LogEnabled()) logger.Log($"Authentication Timeout {player}");

                pendingAuthentication.Remove(player);
                player.Connection?.Disconnect();
            }
        }
    }
}
