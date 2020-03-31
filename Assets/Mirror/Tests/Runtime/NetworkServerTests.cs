using System;
using System.Collections;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using static Mirror.Tests.AsyncUtil;

namespace Mirror.Tests
{
    public class NetworkServerTests
    {
        NetworkServer server;
        GameObject serverGO;

        NetworkConnectionToServer connectionToServer;

        NetworkConnectionToClient connectionToClient;

        [UnitySetUp]
        public IEnumerator SetupNetworkServer()
        {
            return RunAsync(async () =>
            {
                serverGO = new GameObject();
                var transport = serverGO.AddComponent<LoopbackTransport>();
                server = serverGO.AddComponent<NetworkServer>();
                await server.ListenAsync();

                IConnection tconn = await transport.ConnectAsync(new System.Uri("tcp4://localhost"));

                connectionToServer = new NetworkConnectionToServer(tconn);

                connectionToClient = server.connections.First();
            });
        }

        [Test]
        public void InitializeTest()
        {
            Assert.That(server.connections, Has.Count.EqualTo(1));
            Assert.That(server.active);
            Assert.That(server.LocalClientActive, Is.False);
        }

        [Test]
        public void SpawnTest()
        {
            var gameObject = new GameObject();
            gameObject.AddComponent<NetworkIdentity>();
            server.Spawn(gameObject);

            Assert.That(gameObject.GetComponent<NetworkIdentity>().server == server);
        }

        [UnityTest]
        public IEnumerator ReadyMessageSetsClientReadyTest()
        {
            connectionToServer.Send(new ReadyMessage());

            yield return null;

            // ready?
            Assert.That(connectionToClient.isReady, Is.True);
        }

        [UnityTest]
        public IEnumerator SendToAll()
        {
            Action<WovenTestMessage> func = Substitute.For<Action<WovenTestMessage>>();

            connectionToServer.RegisterHandler(func);

            var wovenMessage = new WovenTestMessage
            {
                IntValue = 1,
                DoubleValue = 1.0,
                StringValue = "hello"
            };

            server.SendToAll(wovenMessage);

            _ = connectionToServer.ProcessMessagesAsync();

            yield return null;

            func.Received().Invoke(
                Arg.Is<WovenTestMessage>(msg => msg.Equals(wovenMessage)
            ));
        }

        [UnityTest]
        public IEnumerator RegisterMessage1()
        {
            Action<WovenTestMessage> func = Substitute.For<Action<WovenTestMessage>>();

            connectionToClient.RegisterHandler(func);

            var wovenMessage = new WovenTestMessage
            {
                IntValue = 1,
                DoubleValue = 1.0,
                StringValue = "hello"
            };

            connectionToServer.Send(wovenMessage);

            yield return null;

            func.Received().Invoke(
                Arg.Is<WovenTestMessage>(msg => msg.Equals(wovenMessage)
            ));
        }

        [UnityTest]
        public IEnumerator RegisterMessage2()
        {

            Action<NetworkConnectionToClient, WovenTestMessage> func = Substitute.For<Action<NetworkConnectionToClient, WovenTestMessage>>();

            connectionToClient.RegisterHandler<NetworkConnectionToClient, WovenTestMessage> (func);

            var wovenMessage = new WovenTestMessage
            {
                IntValue = 1,
                DoubleValue = 1.0,
                StringValue = "hello"
            };

            connectionToServer.Send(wovenMessage);

            yield return null;

            func.Received().Invoke(
                connectionToClient,
                Arg.Is<WovenTestMessage>(msg => msg.Equals(wovenMessage)
            ));
        }

        [UnityTest]
        public IEnumerator UnRegisterMessage1()
        {
            Action<WovenTestMessage> func = Substitute.For<Action<WovenTestMessage>>();

            connectionToClient.RegisterHandler(func);
            connectionToClient.UnregisterHandler<WovenTestMessage>();


            var wovenMessage = new WovenTestMessage
            {
                IntValue = 1,
                DoubleValue = 1.0,
                StringValue = "hello"
            };

            connectionToServer.Send(wovenMessage);

            yield return null;

            func.Received(0).Invoke(
                Arg.Any<WovenTestMessage>());
        }

        [TearDown]
        public void ShutdownNetworkServer()
        {
            server.Disconnect();
            GameObject.DestroyImmediate(serverGO);
        }
    }
}
