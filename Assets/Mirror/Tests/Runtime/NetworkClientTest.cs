using System.Collections;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using static Mirror.Tests.AsyncUtil;
using Object = UnityEngine.Object;

namespace Mirror.Tests
{

    [TestFixture]
    public class NetworkClientTest
    {
        NetworkServer server;
        GameObject serverGO;
        NetworkClient client;

        GameObject gameObject;
        NetworkIdentity identity;

        IConnection tconn68;
        IConnection tconn70;

        MockTransport transport;

        TaskCompletionSource<bool> tconn68Receive;
        TaskCompletionSource<bool> tconn70Receive;

        [UnitySetUp]
        public IEnumerator SetUp() => RunAsync(async () =>
        {
            serverGO = new GameObject();
            transport = serverGO.AddComponent<MockTransport>();
            server = serverGO.AddComponent<NetworkServer>();
            client = serverGO.AddComponent<NetworkClient>();

            gameObject = new GameObject();
            identity = gameObject.AddComponent<NetworkIdentity>();

            tconn68 = Substitute.For<IConnection>();
            tconn70 = Substitute.For<IConnection>();

            tconn68Receive = new TaskCompletionSource<bool>();
            tconn70Receive = new TaskCompletionSource<bool>();

            Task<bool> task42 = tconn68Receive.Task;
            Task<bool> task43 = tconn68Receive.Task; //TODO why is this the same receive like on NetworkServerTest?

            tconn68.ReceiveAsync(null).ReturnsForAnyArgs(task42);
            tconn70.ReceiveAsync(null).ReturnsForAnyArgs(task43);

            await server.ListenAsync();
        });

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(gameObject);

            // reset all state
            server.Disconnect();
            Object.DestroyImmediate(serverGO);
        }

        [Test]
        public void IsConnectedTest()
        {
            Assert.That(!client.IsConnected);

            client.ConnectHost(server);

            Assert.That(client.IsConnected);
        }

        [Test]
        public void ConnectionTest()
        {
            Assert.That(client.Connection == null);

            client.ConnectHost(server);

            Assert.That(client.Connection != null);
        }

        [Test]
        public void LocalPlayerTest()
        {
            Assert.That(client.LocalPlayer == null);
        }

        [Test]
        public void CurrentTest()
        {
            Assert.That(NetworkClient.Current == null);
        }
    }
}
