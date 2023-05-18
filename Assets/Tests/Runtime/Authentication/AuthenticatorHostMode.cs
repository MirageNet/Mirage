using System.Collections;
using System.Collections.Generic;
using Mirage.Authentication;
using Mirage.Tests.Runtime.Host;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.Authentication
{
    public class AuthenticatorHostModeBase : HostSetup
    {
        private readonly bool _addAuthenticator;
        private readonly bool _hostRequireAuth;

        protected List<INetworkPlayer> _serverAuthCalls;
        protected List<INetworkPlayer> _clientAuthCalls;
        protected MockAuthenticator _auth;

        public AuthenticatorHostModeBase(bool addAuthenticator, bool hostRequireAuth)
        {
            _addAuthenticator = addAuthenticator;
            _hostRequireAuth = hostRequireAuth;
        }

        protected override void ExtraServerSetup()
        {
            base.ExtraServerSetup();

            if (_addAuthenticator)
            {
                var settings = serverGo.AddComponent<AuthenticatorSettings>();
                settings.RequireHostToAuthenticate = _hostRequireAuth;

                _auth = serverGo.AddComponent<MockAuthenticator>();
                settings.Authenticators.Add(_auth);
                server.Authenticator = settings;
                client.Authenticator = settings;
            }

            // reset fields
            _serverAuthCalls = new List<INetworkPlayer>();
            _clientAuthCalls = new List<INetworkPlayer>();

            server.Authenticated.AddListener(p => _serverAuthCalls.Add(p));
            client.Authenticated.AddListener(p => _clientAuthCalls.Add(p));
        }
    }
    public class NoAuthenticatorHostMode : AuthenticatorHostModeBase
    {
        public NoAuthenticatorHostMode() : base(false, false)
        {
        }

        [Test]
        public void ShouldAuthenticateAfterConnecting()
        {
            Assert.That(_serverAuthCalls, Has.Count.EqualTo(1));
            Assert.That(_serverAuthCalls[0], Is.EqualTo(hostServerPlayer));
            Assert.That(_clientAuthCalls, Has.Count.EqualTo(1));
            Assert.That(_clientAuthCalls[0], Is.EqualTo(hostClientPlayer));

            Assert.That(hostClientPlayer.IsAuthenticated, Is.True);
            Assert.That(hostClientPlayer.Authentication, Is.Not.Null);
            Assert.That(hostClientPlayer.Authentication.Authenticator, Is.Null);

            Assert.That(hostServerPlayer.IsAuthenticated, Is.True);
            Assert.That(hostServerPlayer.Authentication, Is.Not.Null);
            Assert.That(hostServerPlayer.Authentication.Authenticator, Is.Null);
            Assert.That(hostServerPlayer.Authentication.Data, Is.Null);
        }
    }
    public class AuthenticatorHost_NotRequired : AuthenticatorHostModeBase
    {
        public AuthenticatorHost_NotRequired() : base(true, hostRequireAuth: false)
        {
        }

        [Test]
        public void HostShouldAuthenticateWithoutAuthenticator()
        {
            Assert.That(_serverAuthCalls, Has.Count.EqualTo(1));
            Assert.That(_serverAuthCalls[0], Is.EqualTo(hostServerPlayer));
            Assert.That(_clientAuthCalls, Has.Count.EqualTo(1));
            Assert.That(_clientAuthCalls[0], Is.EqualTo(hostClientPlayer));

            Assert.That(hostClientPlayer.IsAuthenticated, Is.True);
            Assert.That(hostClientPlayer.Authentication, Is.Not.Null);
            Assert.That(hostClientPlayer.Authentication.Authenticator, Is.Null);

            Assert.That(hostServerPlayer.IsAuthenticated, Is.True);
            Assert.That(hostServerPlayer.Authentication, Is.Not.Null);
            Assert.That(hostServerPlayer.Authentication.Authenticator, Is.Null);
            Assert.That(hostServerPlayer.Authentication.Data, Is.Null);
        }
    }
    public class AuthenticatorHost_HostRequired : AuthenticatorHostModeBase
    {
        public AuthenticatorHost_HostRequired() : base(true, hostRequireAuth: true)
        {
        }

        [UnityTest]
        public IEnumerator AuthenticatedOnlyAfterMessage()
        {
            Assert.That(_serverAuthCalls, Is.Empty);
            Assert.That(_clientAuthCalls, Is.Empty);

            _auth.SendAuthentication(client, new MockAuthenticator.MockMessage { });

            yield return null;
            yield return null;

            Assert.That(_serverAuthCalls, Has.Count.EqualTo(1));
            Assert.That(_serverAuthCalls[0], Is.EqualTo(server.LocalPlayer));
            Assert.That(_clientAuthCalls, Has.Count.EqualTo(1));
            Assert.That(_clientAuthCalls[0], Is.EqualTo(client.Player));

            Assert.That(client.Player.IsAuthenticated, Is.True);
            Assert.That(client.Player.Authentication, Is.Not.Null);
            Assert.That(client.Player.Authentication.Authenticator, Is.TypeOf<MockAuthenticator>());

            Assert.That(server.LocalPlayer.IsAuthenticated, Is.True);
            Assert.That(server.LocalPlayer.Authentication, Is.Not.Null);
            Assert.That(server.LocalPlayer.Authentication.Authenticator, Is.TypeOf<MockAuthenticator>());
            Assert.That(server.LocalPlayer.Authentication.Data, Is.TypeOf<MockAuthenticator.MockData>());
        }
    }
}
