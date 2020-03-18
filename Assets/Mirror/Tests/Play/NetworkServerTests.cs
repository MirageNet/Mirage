using System.Collections;
using Mirror.Tcp2;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using static Mirror.Tests.AsyncTests;

namespace Mirror.Tests
{
    public class NetworkServerTests 
    {
        NetworkServer server;
        GameObject serverGO;


        [UnitySetUp]
        public IEnumerator SetupNetworkServer()
        {
            serverGO = new GameObject();
            server = serverGO.AddComponent<NetworkServer>();
            serverGO.AddComponent<NetworkClient>();
            server.Transport2 = serverGO.AddComponent<Tcp2Transport>();

            return RunAsync(async () =>
            {
                await server.ListenAsync();
            });
        }

        [TearDown]
        public void ShutdownNetworkServer()
        {
            server.Shutdown();
            Object.DestroyImmediate(serverGO);
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

        [Test]
        public void IsActiveTest()
        {
            Assert.That(server.active, Is.True);
        }

    }
}
