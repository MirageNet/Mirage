using NUnit.Framework;
using static Mirage.Tests.LocalConnections;

namespace Mirage.Tests.Runtime.Host
{
    public class PlayerSceneReadyHostTests : HostSetup<MockComponent>
    {
        [Test]
        public void SetClientReadyAndNotReadyTest()
        {
            (_, NetworkPlayer connection) = PipedConnections(ClientMessageHandler, ServerMessageHandler);
            Assert.That(connection.SceneIsReady, Is.False);

            serverObjectManager.SpawnVisibleObjects(connection);
            Assert.That(connection.SceneIsReady, Is.True);

            sceneManager.SetClientNotReady(connection);
            Assert.That(connection.SceneIsReady, Is.False);
        }

        [Test]
        public void SetAllClientsNotReadyTest()
        {
            // add first ready client
            (_, NetworkPlayer first) = PipedConnections(ClientMessageHandler, ServerMessageHandler);
            first.SceneIsReady = true;
            server.Players.Add(first);

            // add second ready client
            (_, NetworkPlayer second) = PipedConnections(ClientMessageHandler, ServerMessageHandler);
            second.SceneIsReady = true;
            server.Players.Add(second);

            // set all not ready
            sceneManager.SetAllClientsNotReady();
            Assert.That(first.SceneIsReady, Is.False);
            Assert.That(second.SceneIsReady, Is.False);
        }
    }
}
