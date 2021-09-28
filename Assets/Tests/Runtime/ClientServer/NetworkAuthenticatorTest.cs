using System;
using Mirage.Core;
using NSubstitute;
using NUnit.Framework;

namespace Mirage.Tests.Runtime.ClientServer
{
    [TestFixture]
    public class NetworkAuthenticatorTest : ClientServerSetup<MockComponent>
    {
        NetworkAuthenticator serverAuthenticator;
        NetworkAuthenticator clientAuthenticator;

        Action<INetworkPlayer> serverMockMethod;
        Action<INetworkPlayer> clientMockMethod;


        class NetworkAuthenticationImpl : NetworkAuthenticator
        {
            public override void ClientAuthenticate(INetworkPlayer player) => ClientAccept(player);
            public override void ServerAuthenticate(INetworkPlayer player) => ServerAccept(player);
            public override void ClientSetup(NetworkClient client) { }
            public override void ServerSetup(Server server) { }
        }

        public override void ExtraSetup()
        {
            serverAuthenticator = serverGo.AddComponent<NetworkAuthenticationImpl>();
            clientAuthenticator = clientGo.AddComponent<NetworkAuthenticationImpl>();
            server.Authenticator = serverAuthenticator;
            client.authenticator = clientAuthenticator;

            serverMockMethod = Substitute.For<Action<INetworkPlayer>>();
            serverAuthenticator.OnServerAuthenticated += serverMockMethod;

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
            Assert.That(server.Authenticator, Is.EqualTo(serverAuthenticator));
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
