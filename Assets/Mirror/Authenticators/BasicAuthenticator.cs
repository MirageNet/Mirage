using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;

namespace Mirror.Authenticators
{
    [AddComponentMenu("Network/Authenticators/BasicAuthenticator")]
    public class BasicAuthenticator : NetworkAuthenticator
    {
        [Header("Custom Properties")]
        [FormerlySerializedAs("manager")]
        public NetworkManager Manager;

        // set these in the inspector
        public string Username;
        public string Password;

        private class AuthRequestMessage : MessageBase
        {
            // use whatever credentials make sense for your game
            // for example, you might want to pass the accessToken if using oauth
            public string AuthUsername;
            public string AuthPassword;
        }

        private class AuthResponseMessage : MessageBase
        {
            public byte Code;
            public string Message;
        }

        public override void OnStartServer()
        {
            // register a handler for the authentication request we expect from client
            Manager.Server.RegisterHandler<AuthRequestMessage>(OnAuthRequestMessage, false);
        }

        public override void OnStartClient()
        {
            // register a handler for the authentication response we expect from server
            Manager.Client.RegisterHandler<AuthResponseMessage>(OnAuthResponseMessage, false);
        }

        public override void OnServerAuthenticate(NetworkConnectionToClient conn)
        {
            // do nothing...wait for AuthRequestMessage from client
        }

        public override void OnClientAuthenticate(NetworkConnectionToServer conn)
        {
            var authRequestMessage = new AuthRequestMessage
            {
                AuthUsername = Username,
                AuthPassword = Password
            };

            conn.Send(authRequestMessage);
        }

        private void OnAuthRequestMessage(NetworkConnectionToClient conn, AuthRequestMessage msg)
        {
            Debug.LogFormat("Authentication Request: {0} {1}", msg.AuthUsername, msg.AuthPassword);

            // check the credentials by calling your web server, database table, playfab api, or any method appropriate.
            if (msg.AuthUsername == Username && msg.AuthPassword == Password)
            {
                // create and send msg to client so it knows to proceed
                var authResponseMessage = new AuthResponseMessage
                {
                    Code = 100,
                    Message = "Success"
                };

                conn.Send(authResponseMessage);

                // Invoke the event to complete a successful authentication
                base.OnServerAuthenticate(conn);
            }
            else
            {
                // create and send msg to client so it knows to disconnect
                var authResponseMessage = new AuthResponseMessage
                {
                    Code = 200,
                    Message = "Invalid Credentials"
                };

                conn.Send(authResponseMessage);

                // must set NetworkConnection isAuthenticated = false
                conn.isAuthenticated = false;

                // disconnect the client after 1 second so that response message gets delivered
                StartCoroutine(DelayedDisconnect(conn, 1));
            }
        }

        private IEnumerator DelayedDisconnect(NetworkConnection conn, float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            conn.Disconnect();
        }

        private void OnAuthResponseMessage(NetworkConnectionToServer conn, AuthResponseMessage msg)
        {
            if (msg.Code == 100)
            {
                Debug.LogFormat("Authentication Response: {0}", msg.Message);

                // Invoke the event to complete a successful authentication
                base.OnClientAuthenticate(conn);
            }
            else
            {
                Debug.LogErrorFormat("Authentication Response: {0}", msg.Message);

                // Set this on the client for local reference
                conn.isAuthenticated = false;

                // disconnect the client
                conn.Disconnect();
            }
        }
    }
}
