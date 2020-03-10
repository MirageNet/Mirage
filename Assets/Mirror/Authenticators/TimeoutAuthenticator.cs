using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace Mirror.Authenticators
{
    /// <summary>
    /// An authenticator that disconnects connections if they don't
    /// authenticate within a specified time limit.
    /// </summary>
    [AddComponentMenu("Network/Authenticators/TimeoutAuthenticator")]
    public class TimeoutAuthenticator : NetworkAuthenticator
    {
        public NetworkAuthenticator Authenticator;

        [FormerlySerializedAs("Timeout")]
        [Range(0, 600), Tooltip("Timeout to auto-disconnect in seconds. Set to 0 for no timeout.")]
        [SerializeField] private float timeout = 60;

        public void Awake()
        {
            Authenticator.OnClientAuthenticated += base.OnClientAuthenticate;
            Authenticator.OnServerAuthenticated += base.OnServerAuthenticate;
        }

        public override void OnClientAuthenticate(NetworkConnectionToServer conn)
        {
            Authenticator.OnClientAuthenticate(conn);

            if (timeout > 0)
                StartCoroutine(BeginAuthentication(conn));
        }

        public override void OnServerAuthenticate(NetworkConnectionToClient conn)
        {
            Authenticator.OnServerAuthenticate(conn);

            if (timeout > 0)
                StartCoroutine(BeginAuthentication(conn));
        }

        private IEnumerator BeginAuthentication(NetworkConnection conn)
        {
            if (LogFilter.Debug)
                Debug.Log($"Authentication countdown started {conn} {timeout}");

            yield return new WaitForSecondsRealtime(timeout);

            if (conn.isAuthenticated)
                yield break;

            if (LogFilter.Debug)
                Debug.Log($"Authentication Timeout {conn}");

            conn.Disconnect();
        }
    }
}
