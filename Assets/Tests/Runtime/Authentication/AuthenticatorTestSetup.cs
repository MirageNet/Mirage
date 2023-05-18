using System.Collections.Generic;
using Mirage.Authentication;

namespace Mirage.Tests.Runtime.Authentication
{
    public abstract class AuthenticatorTestSetup<T> : ClientServerSetup where T : NetworkAuthenticator
    {
        protected AuthenticatorSettings _serverSettings;
        protected T _serverAuthenticator;
        protected AuthenticatorSettings _clientSettings;
        protected T _clientAuthenticator;

        protected List<INetworkPlayer> _serverAuthCalls;
        protected List<INetworkPlayer> _clientAuthCalls;


        protected override void ExtraServerSetup()
        {
            base.ExtraServerSetup();

            _serverSettings = serverGo.AddComponent<AuthenticatorSettings>();
            _serverAuthenticator = serverGo.AddComponent<T>();
            _serverSettings.Authenticators.Add(_serverAuthenticator);

            server.Authenticator = _serverSettings;
            _serverAuthCalls = new List<INetworkPlayer>();
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
            _clientAuthCalls = new List<INetworkPlayer>();
            client.Authenticated.AddListener(p => _clientAuthCalls.Add(p));
        }
    }
}
