using System;
using System.Linq;
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

        public void Start()
        {
            Client.Connected.AddListener(ClientConnected);
            Client.Authenticated.AddListener(ClientAuthenticated);

            Server.Started.AddListener(ServerStarted);
        }

        private void ServerStarted()
        {
            Server.MessageHandler.RegisterHandler<RequestSessionMessage>(HandleRequestSession);
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
            if (!Authenticator.ClientIdStore.TryGetSession(out _))
            {
                RequestSession();
            }
        }

        private void RequestSession()
        {
            var waiter = new MessageWaiter<SessionKeyMessage>(Client.MessageHandler, allowUnauthenticated: false);

            Client.Send(new RequestSessionMessage { });

            waiter.Callback((player, msg) =>
            {
                // copy to new array, because ArraySegment will be reused aft
                var key = msg.SessionKey.ToArray();
                var session = new ClientSession
                {
                    Key = key,
                    Timeout = DateTime.Now.AddMinutes(Authenticator.TimeoutMinutes),
                };

                Authenticator.ClientIdStore.StoreSession(session);
            });
        }

        public void HandleRequestSession(INetworkPlayer player, RequestSessionMessage message)
        {
            var sessionKey = Authenticator.CreateOrRefreshSession(player);
            player.Send(new SessionKeyMessage { SessionKey = sessionKey });
        }

        private void Update()
        {
            CheckRefresh();
        }

        private void CheckRefresh()
        {
            if (Client == null || !Client.Active)
                return;

            if (!Authenticator.ClientIdStore.TryGetSession(out var session))
                return;

            var tillRefresh = TimeSpan.FromMinutes(Authenticator.TimeoutMinutes / 2);
            if (session.NeedsRefreshing(tillRefresh))
            {
                RequestSession();
            }
        }
    }
}
