using Mirror.Tcp;
using NUnit.Framework;
using UnityEngine;

namespace Mirror.Tests
{
    public class NetworkServerTests 
    {
        NetworkServer server;
        GameObject serverGO;

        [SetUp]
        public void SetupNetworkServer()
        {
            serverGO = new GameObject();
            server = serverGO.AddComponent<NetworkServer>();
            serverGO.AddComponent<NetworkClient>();
            Transport transport = serverGO.AddComponent<TcpTransport>();

            Transport.activeTransport = transport;
            server.Listen();
        }

        [Test]
        public void InitializeTest()
        {
            Assert.That(server.connections, Is.Empty);
            Assert.That(server.active);
            Assert.That(server.LocalClientActive, Is.False);
        }

        [Test]
        public void SpawnTest()
        {
            var gameObject = new GameObject();
            gameObject.AddComponent<NetworkIdentity>();
            server.Spawn(gameObject);

            Assert.That(gameObject.GetComponent<NetworkIdentity>().server, Is.SameAs(server));
        }

       
        [Test]
        public void ShutdownTest()
        {
            server.Shutdown();
            Assert.That(server.active, Is.False);
        }

        [TearDown]
        public void ShutdownNetworkServer()
        {
            server.Shutdown();
            GameObject.DestroyImmediate(serverGO);
        }
    }
}
