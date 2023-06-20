using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
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

        protected override bool SpawnCharacterOnConnect => false;

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

    public class AuthenticatorHost_NoAutoStart : AuthenticatorHostModeBase
    {
        // dont auto start, we need to add events first
        protected override bool StartServer => false;

        public AuthenticatorHost_NoAutoStart() : base(false, false)
        {
        }

        [UnityTest]
        public IEnumerator BothShouldBeAuthenticatedBeforeEventIsCalled() => UniTask.ToCoroutine(async () =>
        {
            var serverAuthCalled = 0;
            var clientAuthCalled = 0;
            // need both server and client players to be marked as authenticated before events are invoked
            // this is because message are invoked instantly in host mode,
            // because of this players need to be marked as authenticated to avoid "Disconnecting Unauthenticated player" error
            server.Authenticated.AddListener((player) =>
            {
                logger.Log("Server Authenticated");
                // check both server and client player are authed
                Assert.That(client.Player.IsAuthenticated, Is.True);
                Assert.That(server.LocalPlayer.IsAuthenticated, Is.True);
                serverAuthCalled++;
            });
            client.Authenticated.AddListener((player) =>
            {
                logger.Log("Client Authenticated");
                Assert.That(client.Player.IsAuthenticated, Is.True);
                Assert.That(server.LocalPlayer.IsAuthenticated, Is.True);
                clientAuthCalled++;
            });

            _serverInstance.StartServer();

            await AsyncUtil.WaitUntilWithTimeout(() => serverAuthCalled > 0 && clientAuthCalled > 0);

            Assert.That(serverAuthCalled, Is.EqualTo(1));
            Assert.That(clientAuthCalled, Is.EqualTo(1));
        });
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

            Assert.That(server.LocalPlayer.IsAuthenticated, Is.True);
            Assert.That(server.LocalPlayer.Authentication, Is.Not.Null);
            Assert.That(server.LocalPlayer.Authentication.Authenticator, Is.TypeOf<MockAuthenticator>());
            Assert.That(server.LocalPlayer.Authentication.Data, Is.TypeOf<MockAuthenticator.MockData>());

            // client needs extra frame to receive message from server
            yield return null;

            Assert.That(_clientAuthCalls, Has.Count.EqualTo(1));
            Assert.That(_clientAuthCalls[0], Is.EqualTo(client.Player));

            Assert.That(client.Player.IsAuthenticated, Is.True);
            Assert.That(client.Player.Authentication, Is.Not.Null);
            Assert.That(client.Player.Authentication.Authenticator, Is.TypeOf<MockAuthenticator>());

        }
    }

    public class AuthenticatorHost_HostRequired_ClientConnect : AuthenticatorHostModeBase
    {
        public AuthenticatorHost_HostRequired_ClientConnect() : base(true, hostRequireAuth: true)
        {
        }

        protected override void ExtraClientSetup(IClientInstance instance)
        {
            base.ExtraClientSetup(instance);

            instance.Client.Connected.AddListener(ClientConnected);
        }

        private void ClientConnected(INetworkPlayer arg0)
        {
            // check that we can send auth when client connects
            _auth.SendAuthentication(client, new MockAuthenticator.MockMessage { });
        }

        [UnityTest]
        public IEnumerator AuthenticatesFromMessageSentInConnected()
        {
            yield return null;
            yield return null;

            Assert.That(_serverAuthCalls, Has.Count.EqualTo(1));
            Assert.That(_serverAuthCalls[0], Is.EqualTo(server.LocalPlayer));

            Assert.That(server.LocalPlayer.IsAuthenticated, Is.True);
            Assert.That(server.LocalPlayer.Authentication, Is.Not.Null);
            Assert.That(server.LocalPlayer.Authentication.Authenticator, Is.TypeOf<MockAuthenticator>());
            Assert.That(server.LocalPlayer.Authentication.Data, Is.TypeOf<MockAuthenticator.MockData>());

            // client needs extra frame to receive message from server
            yield return null;

            Assert.That(_clientAuthCalls, Has.Count.EqualTo(1));
            Assert.That(_clientAuthCalls[0], Is.EqualTo(client.Player));

            Assert.That(client.Player.IsAuthenticated, Is.True);
            Assert.That(client.Player.Authentication, Is.Not.Null);
            Assert.That(client.Player.Authentication.Authenticator, Is.TypeOf<MockAuthenticator>());

        }
    }
}
