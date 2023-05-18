using System;
using Mirage.Authentication;
using Mirage.Tests.Runtime.ClientServer;
using NUnit.Framework;

namespace Mirage.Tests.Runtime.Host.Authenticators
{
    [TestFixture((Type)null, true)]
    [TestFixture((Type)null, false)]
    [TestFixture(typeof(MockAuthenticator), true)]
    [TestFixture(typeof(MockAuthenticator), false)]
    public class AuthenticatorHostMode : HostSetup
    {
        private readonly Type _authType;
        private readonly bool _requireAuth;

        private int serverAuthCalled;
        private int clientAuthCalled;

        public AuthenticatorHostMode(Type authType, bool requireAuth)
        {
            _authType = authType;
            _requireAuth = requireAuth;
        }

        protected override void ExtraServerSetup()
        {
            base.ExtraServerSetup();

            if (_authType != null)
            {
                var settings = serverGo.AddComponent<AuthenticatorSettings>();
                settings.RequireHostToAuthenticate = _requireAuth;

                var auth = serverGo.AddComponent(_authType) as NetworkAuthenticator;
                settings.Authenticators.Add(auth);
                server.Authenticator = settings;
                client.Authenticator = settings;
            }

            // reset fields
            serverAuthCalled = 0;
            clientAuthCalled = 0;

            server.Authenticated.AddListener(_ => serverAuthCalled++);
            client.Authenticated.AddListener(_ => clientAuthCalled++);
        }

        [Test]
        public void AuthenticatedCalledOnceOnServer()
        {
            Assert.That(serverAuthCalled, Is.EqualTo(1));
            Assert.That(server.LocalPlayer, Is.Not.Null);
            Assert.That(server.LocalPlayer.IsAuthenticated, Is.True);
            Assert.That(server.LocalPlayer.Authentication, Is.Not.Null);

            if (_requireAuth)
            {
                Assert.That(server.LocalPlayer.Authentication.Authenticator, Is.TypeOf<MockAuthenticator>());
                Assert.That(server.LocalPlayer.Authentication.Data, Is.TypeOf<MockAuthenticator.MockData>());
            }
            else
            {
                Assert.That(server.LocalPlayer.Authentication.Authenticator, Is.Null, "host should have skipped auth");
                Assert.That(server.LocalPlayer.Authentication.Data, Is.Null, "host should have skipped auth");
            }
        }

        [Test]
        public void AuthenticatedCalledOnceOnClient()
        {
            Assert.That(clientAuthCalled, Is.EqualTo(1));
            Assert.That(client.Player, Is.Not.Null);
            Assert.That(client.Player.IsAuthenticated, Is.True);
            Assert.That(client.Player.Authentication, Is.Not.Null);

            if (_requireAuth)
            {
                Assert.That(client.Player.Authentication.Authenticator, Is.TypeOf<MockAuthenticator>());
            }
            else
            {
                Assert.That(client.Player.Authentication.Authenticator, Is.Null, "host should have skipped auth");
            }
        }
    }
}
