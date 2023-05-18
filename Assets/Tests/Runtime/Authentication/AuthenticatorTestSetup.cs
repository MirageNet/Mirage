using System.Collections.Generic;
using Mirage.Authentication;

namespace Mirage.Tests.Runtime.ClientServer
{
    public abstract class AuthenticatorTestSetup<T> : ClientServerSetup where T : NetworkAuthenticator
    {
        protected AuthenticatorSettings _serverSettings;
        protected T _serverAuthenticator;
        protected AuthenticatorSettings _clientSettings;
        protected T _clientAuthenticator;

        private readonly List<INetworkPlayer> _serverAuthCalls = new List<INetworkPlayer>();
        private readonly List<INetworkPlayer> _clientAuthCalls = new List<INetworkPlayer>();

        protected override void ExtraServerSetup()
        {
            base.ExtraServerSetup();

            _serverSettings = serverGo.AddComponent<AuthenticatorSettings>();
            _serverAuthenticator = serverGo.AddComponent<T>();
            _serverSettings.Authenticators.Add(_serverAuthenticator);

            server.Authenticator = _serverSettings;
            server.Authenticated.AddListener(p => _serverAuthCalls.Add(p));
        }
        protected override void ExtraClientSetup(IClientInstance instance)
        {
            base.ExtraClientSetup(instance);
            var client = instance.Client;

            _clientSettings = clientGo.AddComponent<AuthenticatorSettings>();
            _clientAuthenticator = clientGo.AddComponent<T>();
            _clientSettings.Authenticators.Add(_clientAuthenticator);

            client.Authenticator = _clientSettings;
            client.Authenticated.AddListener(p => _clientAuthCalls.Add(p));
        }
    }
}
