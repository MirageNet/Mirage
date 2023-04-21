using System;
using NSubstitute;
using NUnit.Framework;

namespace Mirage.Tests.Runtime.ClientServer
{
    public class MockAuthentication : NetworkAuthenticator
    {
        public override void ClientAuthenticate(INetworkPlayer player) => ClientAccept(player);
        public override void ServerAuthenticate(INetworkPlayer player) => ServerAccept(player);
        public override void ClientSetup(NetworkClient client) { }
        public override void ServerSetup(NetworkServer server) { }
    }

    public class NetworkAuthenticatorTest : ClientServerSetup
    {
        private NetworkAuthenticator serverAuthenticator;
        private NetworkAuthenticator clientAuthenticator;
        private Action<INetworkPlayer> serverMockMethod;
        private Action<INetworkPlayer> clientMockMethod;

        protected override void ExtraServerSetup()
        {
            base.ExtraServerSetup();

            serverAuthenticator = serverGo.AddComponent<MockAuthentication>();
            server.authenticator = serverAuthenticator;
            serverMockMethod = Substitute.For<Action<INetworkPlayer>>();
            serverAuthenticator.OnServerAuthenticated += serverMockMethod;
        }
        protected override void ExtraClientSetup(IClientInstance instance)
        {
            base.ExtraClientSetup(instance);
            var client = instance.Client;

            clientAuthenticator = instance.GameObject.AddComponent<MockAuthentication>();
            client.authenticator = clientAuthenticator;
            clientMockMethod = Substitute.For<Action<INetworkPlayer>>();
            clientAuthenticator.OnClientAuthenticated += clientMockMethod;
        }

        [Test]
        public void OnServerAuthenticateTest()
        {
            serverAuthenticator.ServerAuthenticate(Substitute.For<INetworkPlayer>());

            serverMockMethod.Received().Invoke(Arg.Any<INetworkPlayer>());
        }

        [Test]
        public void OnClientAuthenticateTest()
        {
            clientAuthenticator.ClientAuthenticate(Substitute.For<INetworkPlayer>());

            clientMockMethod.Received().Invoke(Arg.Any<INetworkPlayer>());
        }

        [Test]
        public void ClientOnValidateTest()
        {
            Assert.That(client.authenticator, Is.EqualTo(clientAuthenticator));
        }

        [Test]
        public void ServerOnValidateTest()
        {
            Assert.That(server.authenticator, Is.EqualTo(serverAuthenticator));
        }

        [Test]
        public void NetworkClientCallsAuthenticator()
        {
            clientMockMethod.Received().Invoke(Arg.Any<INetworkPlayer>());
        }

        [Test]
        public void NetworkServerCallsAuthenticator()
        {
            clientMockMethod.Received().Invoke(Arg.Any<INetworkPlayer>());
        }
    }
}
