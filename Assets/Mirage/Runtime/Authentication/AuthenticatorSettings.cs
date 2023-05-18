using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Mirage.Logging;
using UnityEngine;

namespace Mirage.Authentication
{
    public sealed class AuthenticatorSettings : MonoBehaviour
    {
        private static readonly ILogger logger = LogFactory.GetLogger<AuthenticatorSettings>();

        public int TimeoutSeconds = 60;

        [Tooltip("Should the host player authenticate? If this is false then they will be marked as Authenticated automatically without going through a NetworkAuthenticator")]
        public bool RequireHostToAuthenticate;

        [Tooltip("List of Authenticators allowed, User can use any of them")]
        public List<NetworkAuthenticator> Authenticators = new List<NetworkAuthenticator>();

        private readonly Dictionary<INetworkPlayer, UniTaskCompletionSource<AuthenticationResult>> _pending = new Dictionary<INetworkPlayer, UniTaskCompletionSource<AuthenticationResult>>();

        private MessageHandler _authHandler;
        private NetworkServer _server;

        public void Setup(NetworkServer server)
        {
            if (_server != null && _server != server)
                throw new InvalidOperationException($"ServerAuthenticator already in use by another NetworkServer, current:{_server}, new:{server}");
            _server = server;

            server.MessageHandler.RegisterHandler<AuthMessage>(HandleAuthMessage, allowUnauthenticated: true);

            // message handler used just for Auth message
            // this is needed because message are wrapped inside AuthMessage
            _authHandler = new MessageHandler(null, true);

            server.Disconnected.AddListener(ServerDisconnected);

            foreach (var authenticator in Authenticators)
            {
                authenticator.Setup(_authHandler, AfterAuth);
            }
        }

        private void HandleAuthMessage(INetworkPlayer player, AuthMessage authMessage)
        {
            _authHandler.HandleMessage(player, authMessage.Payload);
        }

        private void ServerDisconnected(INetworkPlayer player)
        {
            // if player is pending, then set their result to fail
            if (_pending.TryGetValue(player, out var taskCompletion))
            {
                taskCompletion.TrySetResult(AuthenticationResult.CreateFail("Disconnected"));
            }
        }

        public async UniTask<AuthenticationResult> ServerAuthenticate(INetworkPlayer player)
        {
            if (logger.LogEnabled()) logger.Log($"Server authentication started {player}");

            var result = await RunServerAuthenticate(player);

            if (logger.LogEnabled())
            {
                var successText = result.Success ? "success" : "failed";
                var authenticatorName = result.Authenticator?.AuthenticatorName ?? "Null";
                logger.Log($"Server authentication {successText} {player}, Reason:{result.Reason}, Authenticator:{authenticatorName}");
            }

            return result;
        }

        private async UniTask<AuthenticationResult> RunServerAuthenticate(INetworkPlayer player)
        {
            var taskCompletion = new UniTaskCompletionSource<AuthenticationResult>();
            _pending.Add(player, taskCompletion);

            try
            {
                // need cancel for when player disconnects
                (var isTimeout, var result) = await taskCompletion.Task
                    .TimeoutWithoutException(TimeSpan.FromSeconds(TimeoutSeconds), delayType: DelayType.UnscaledDeltaTime);

                if (isTimeout)
                {
                    return AuthenticationResult.CreateFail("Timeout");
                }

                return result;
            }
            catch (Exception e)
            {
                logger.LogException(e);
                return AuthenticationResult.CreateFail($"Exception {e.GetType()}");
            }
            finally
            {
                _pending.Remove(player);
            }
        }

        private void AfterAuth(INetworkPlayer player, AuthenticationResult result)
        {
            if (_pending.TryGetValue(player, out var taskCompletion))
            {
                taskCompletion.TrySetResult(result);
            }
            else
            {
                logger.LogError("Received AfterAuth Callback from player that was not in pending authentication");
            }
        }
    }
}

