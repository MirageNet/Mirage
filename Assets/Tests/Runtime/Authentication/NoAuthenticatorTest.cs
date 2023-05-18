using System.Collections.Generic;
using NUnit.Framework;

namespace Mirage.Tests.Runtime.Authentication
{
    public class NoAuthenticatorTest : ClientServerSetup
    {
        protected List<INetworkPlayer> _serverAuthCalls;
        protected List<INetworkPlayer> _clientAuthCalls;

        protected override void ExtraServerSetup()
        {
            base.ExtraServerSetup();

            _serverAuthCalls = new List<INetworkPlayer>();
            server.Authenticated.AddListener(p => _serverAuthCalls.Add(p));
        }
        protected override void ExtraClientSetup(IClientInstance instance)
        {
            base.ExtraClientSetup(instance);
            var client = instance.Client;

            _clientAuthCalls = new List<INetworkPlayer>();
            client.Authenticated.AddListener(p => _clientAuthCalls.Add(p));
        }

        [Test]
        public void ShouldAuthenticateAfterConnecting()
        {
            Assert.That(_serverAuthCalls, Has.Count.EqualTo(1));
            Assert.That(_serverAuthCalls[0], Is.EqualTo(serverPlayer));
            Assert.That(_clientAuthCalls, Has.Count.EqualTo(1));
            Assert.That(_clientAuthCalls[0], Is.EqualTo(clientPlayer));

            Assert.That(clientPlayer.IsAuthenticated, Is.True);
            Assert.That(clientPlayer.Authentication, Is.Not.Null);
            Assert.That(clientPlayer.Authentication.Authenticator, Is.Null);

            Assert.That(serverPlayer.IsAuthenticated, Is.True);
            Assert.That(serverPlayer.Authentication, Is.Not.Null);
            Assert.That(clientPlayer.Authentication.Authenticator, Is.Null);
            Assert.That(clientPlayer.Authentication.Data, Is.Null);
        }
    }
}
