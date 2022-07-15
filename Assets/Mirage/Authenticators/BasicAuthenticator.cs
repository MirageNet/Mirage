using System.Collections;
using Mirage.Logging;
using UnityEngine;

namespace Mirage.Authenticators
{
    /// <summary>
    /// Basic Authenticator that lets the server/host set a "passcode" in order to connect.
    /// <para>
    /// This code could be a short string that can be used to host a private game.
    /// The host would set the code and then give it to their friends allowing them to join.
    /// </para>
    /// </summary>
    [AddComponentMenu("Network/Authenticators/BasicAuthenticator")]
    public class BasicAuthenticator : NetworkAuthenticator
    {
        private static readonly ILogger logger = LogFactory.GetLogger(typeof(BasicAuthenticator));

        /// <summary>
        /// Code given to clients so that they can connect to the server/host
        /// <para>
        /// Set this in inspector or at runtime when the server/host starts
        /// </para>
        /// </summary>
        [Header("Custom Properties")]
        public string serverCode;


        /// <summary>
        /// Use whatever credentials make sense for your game.
        /// <para>
        ///     This example uses a code so that only players that know the code can join.
        /// </para>
        /// <para>
        ///     You might want to use an accessToken or passwords. Be aware that the normal connection
        ///     in Mirage is not encrypted so sending secure information directly is not adviced
        /// </para>
        /// </summary>

        [NetworkMessage]
        private struct AuthRequestMessage
        {
            public string serverCode;
        }

        [NetworkMessage]
        private struct AuthResponseMessage
        {
            public bool success;
            public string message;
        }


        #region Server Authenticate

        public override void ServerSetup(NetworkServer server)
        {
            // register messsage for Auth when server starts
            // this will ensure the handlers are ready when client connects (even in host mode)

            server.MessageHandler.RegisterHandler<AuthRequestMessage>(OnAuthRequestMessage);
        }

        public override void ServerAuthenticate(INetworkPlayer player)
        {
            // wait for AuthRequestMessage from client
        }

        private void OnAuthRequestMessage(INetworkPlayer player, AuthRequestMessage msg)
        {
            if (logger.LogEnabled()) logger.LogFormat(LogType.Log, "Authentication Request: {0} {1}", msg.serverCode);

            // check if client send the same code as the one stored in the server
            if (msg.serverCode == serverCode)
            {
                // create and send msg to client so it knows to proceed
                player.Send(new AuthResponseMessage
                {
                    success = true,
                    message = "Success"
                });

                ServerAccept(player);
            }
            else
            {
                // create and send msg to client so it knows to disconnect
                var authResponseMessage = new AuthResponseMessage
                {
                    success = false,
                    message = "Invalid code"
                };

                ServerReject(player);

                player.Send(authResponseMessage);

                // disconnect the client after 1 second so that response message gets delivered
                StartCoroutine(DelayedDisconnect(player, 1));
            }
        }

        private IEnumerator DelayedDisconnect(INetworkPlayer player, float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            player.Disconnect();
        }

        #endregion

        #region Client Authenticate

        public override void ClientSetup(NetworkClient client)
        {
            // register messsage for Auth when client starts
            // this will ensure the handlers are ready when client connects (even in host mode)

            client.MessageHandler.RegisterHandler<AuthResponseMessage>(OnAuthResponseMessage);
        }

        public override void ClientAuthenticate(INetworkPlayer player)
        {
            // The serverCode should be set on the client before connection to the server.
            // When the client connects it sends the code and the server checks that it is correct
            player.Send(new AuthRequestMessage
            {
                serverCode = serverCode,
            });
        }

        private void OnAuthResponseMessage(INetworkPlayer player, AuthResponseMessage msg)
        {
            if (msg.success)
            {
                if (logger.LogEnabled()) logger.LogFormat(LogType.Log, "Authentication Success: {0}", msg.message);
                ClientAccept(player);
            }
            else
            {
                logger.LogFormat(LogType.Error, "Authentication Fail: {0}", msg.message);
                ClientReject(player);
            }
        }

        #endregion
    }
}
