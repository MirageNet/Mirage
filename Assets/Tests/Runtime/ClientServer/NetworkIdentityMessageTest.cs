using NUnit.Framework;

namespace Mirage.Tests.Runtime.ClientServer
{
    public class NetworkIdentityMessageTest : ClientServerSetup<MockComponent>
    {
        [NetworkMessage]
        public struct MyMessage
        {
            public NetworkIdentity player1;
        }

        [Test]
        public void MessageFindsNetworkIdentities()
        {
            NetworkIdentity found = null;
            client.MessageHandler.RegisterHandler((MyMessage msg) =>
            {
                found = msg.player1;
            });
            serverPlayer.Send(new MyMessage { player1 = serverPlayer.Identity });

            server.Update();
            client.Update();

            Assert.That(found == clientPlayer.Identity, "Could not find client version of object");
        }
    }
}
