using System;
using UnityEngine;

namespace Mirage.Authenticators.SessionId
{
    /// <summary>
    /// Creates a session to be used by <see cref="SessionIdAuthenticator"/>
    /// </summary>
    public class CreateSession : MonoBehaviour
    {
        public NetworkServer Server;
        public NetworkClient Client;
        public SessionIdAuthenticator Authenticator;

        [Tooltip("Is the player required to be authenticated by another Authenticator before starting Session")]
        public bool RequestAuthenticated = true;

        public void Start()
        {
            Client.Connected.AddListener(ClientConnected);
            Client.Authenticated.AddListener(ClientAuthenticated);
        }

        private void ClientConnected(INetworkPlayer player)
        {
            if (Authenticator.ClientIdStore.TryGetSession(out var session))
            {
                // if before timeout, then use it to authenticate
                if (DateTime.Now < session.Timeout)
                    SendAuthentication(session);
            }
        }

        private void SendAuthentication(ClientSession session)
        {
            var msg = new SessionKeyMessage
            {
                SessionKey = new ArraySegment<byte>(session.Key)
            };
            Authenticator.SendAuthentication(Client, msg);
        }

        private void ClientAuthenticated(INetworkPlayer player)
        {
            // check if we have session ID, otherwise request one here
        }

        public struct RequestSession
        {
            public bool RefreshExisting;
        }


        // Add code to do the following:
        // - use existing token too authenticate when reconnecting
        // - get session token after authenticate (if client doesn't have one)
        // - refresh token if only 1/2 time is remaining
        // - store token in ClientIdStore
    }
}
