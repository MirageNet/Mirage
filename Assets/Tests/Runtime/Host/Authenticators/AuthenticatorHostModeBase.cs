using System;
using Mirage.Authenticators;
using NUnit.Framework;

namespace Mirage.Tests.Runtime.Host.Authenticators
{
    [TestFixture(arguments: (Type)null)]
    [TestFixture(arguments: typeof(BasicAuthenticator))]
    public class AuthenticatorHostMode : HostSetup
    {
        private readonly Type _authType;
        private int serverAuthCalled;
        private int clientAuthCalled;

        public AuthenticatorHostMode(Type authType)
        {
            _authType = authType;
        }

        protected override void ExtraServerSetup()
        {
            base.ExtraServerSetup();

            if (_authType != null)
            {
                var auth = serverGo.AddComponent(_authType) as NetworkAuthenticator;
                server.authenticator = auth;
                client.authenticator = auth;

                if (auth is BasicAuthenticator basic)
                    basic.serverCode = "1234";
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
        }

        [Test]
        public void AuthenticatedCalledOnceOnClient()
        {
            Assert.That(clientAuthCalled, Is.EqualTo(1));
        }
    }
}
