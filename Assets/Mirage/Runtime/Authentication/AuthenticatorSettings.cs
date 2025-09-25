using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

        private readonly Dictionary<INetworkPlayer, PendingAuth> _pending = new Dictionary<INetworkPlayer, PendingAuth>();
        /// <summary>Set to make sure player can't send 2 AuthMessage</summary>
        private readonly HashSet<INetworkPlayer> _hasSentAuth = new HashSet<INetworkPlayer>();

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
            _authHandler = new MessageHandler(null, true, _server.RethrowException);

            server.Disconnected.AddListener(ServerDisconnected);
            server.Stopped.AddListener(ServerStopped);

            foreach (var authenticator in Authenticators)
            {
                // there might be a null entry, warn and skip to avoid an NRE.
                if (authenticator == null)
                {
                    logger.LogWarning("Null/missing authenticator detected in Network Authenticator list!");
                    continue;
                }

                authenticator.Setup(this, _authHandler, AfterAuth);
            }
        }

        private void ServerStopped()
        {
            if (_pending.Count > 0)
            {
                foreach (var pending in _pending.Values.ToArray()) // ToArray because SetResult will remove from _pending
                    pending.SetResult(AuthenticationResult.CreateFail("Server Stopped"));
            }
            _pending.Clear();
            _hasSentAuth.Clear();
        }

        private void HandleAuthMessage(INetworkPlayer player, AuthMessage authMessage)
        {
            // Check if player has already sent an auth message
            var added = _hasSentAuth.Add(player);
            if (!added)
            {
                if (logger.WarnEnabled()) logger.LogWarning($"Player {player} attempted to send multiple authentication messages");
                return;
            }

            _authHandler.HandleMessage(player, authMessage.Payload);
        }

        private void ServerDisconnected(INetworkPlayer player)
        {
            // Clean up tracking for disconnected player
            _hasSentAuth.Remove(player);

            // if player is pending, then set their result to fail
            if (_pending.TryGetValue(player, out var pendingAuth))
            {
                pendingAuth.SetResult(AuthenticationResult.CreateFail("Disconnected"));
            }
        }

        public async UniTask<AuthenticationResult> ServerAuthenticate(INetworkPlayer player)
        {
            if (SkipHost(player))
                return AuthenticationResult.CreateSuccess("Host player");

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

        private bool SkipHost(INetworkPlayer player)
        {
            var isHost = player == _server.LocalPlayer;

            if (!isHost)
                return false;

            var skip = !RequireHostToAuthenticate;
            return skip;
        }

        private async UniTask<AuthenticationResult> RunServerAuthenticate(INetworkPlayer player)
        {
            PendingAuth pendingAuth;
            // host player should be added by PreAddHostPlayer, so we just get item
            if (player == _server.LocalPlayer)
            {
                pendingAuth = _pending[player];
            }
            // remote player should add new token here
            else
            {
                pendingAuth = new PendingAuth();
                _pending.Add(player, pendingAuth);
            }

            try
            {
                var result = await pendingAuth.WaitWithTimeout(TimeoutSeconds);
                return result;
            }
            catch (Exception e)
            {
                logger.LogException(e);
                return AuthenticationResult.CreateFail($"Exception {e.GetType()}");
            }
            finally
            {
                pendingAuth.CancelSource.Cancel();
                _pending.Remove(player);
            }
        }

        public CancellationToken GetCancellationToken(INetworkPlayer player)
        {
            if (_pending.TryGetValue(player, out var result))
                return result.CancelSource.Token;
            else
                throw new ArgumentException("Can't get CancellationToken for player that is not pending");
        }

        internal void AfterAuth(INetworkPlayer player, AuthenticationResult result)
        {
            if (_pending.TryGetValue(player, out var pendingAuth))
            {
                pendingAuth.SetResult(result);
            }
            else
            {
                logger.LogError("Received AfterAuth Callback from player that was not in pending authentication");
            }
        }

        internal void PreAddHostPlayer(INetworkPlayer player)
        {
            // dont add if host dont require auth
            if (!RequireHostToAuthenticate)
                return;

            // host player is a special case, they are added early
            // otherwise Client.Connected can't be used to send auth message
            // because that is called before RunServerAuthenticate is called.
            _pending.Add(player, new PendingAuth());
        }

        public class PendingAuth
        {
            private readonly UniTaskCompletionSource<AuthenticationResult> _result = new UniTaskCompletionSource<AuthenticationResult>();
            public readonly CancellationTokenSource CancelSource = new CancellationTokenSource();

            public void SetResult(AuthenticationResult result)
            {
                _result.TrySetResult(result);
            }

            public async UniTask<AuthenticationResult> WaitWithTimeout(float timeoutSecond)
            {
                // need cancel for when player disconnects
                var (isCanceled, (isTimeout, result)) = await _result.Task
                    .AttachExternalCancellation(CancelSource.Token)
                    .TimeoutWithoutException(TimeSpan.FromSeconds(timeoutSecond), delayType: DelayType.UnscaledDeltaTime)
                    .SuppressCancellationThrow();

                if (isTimeout)
                    return AuthenticationResult.CreateFail("Timeout");

                if (isCanceled)
                    return AuthenticationResult.CreateFail("Cancelled");

                return result;
            }
        }
    }
}

