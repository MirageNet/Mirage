using Mirror.Tcp;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using static Mirror.Tests.AsyncUtil;

namespace Mirror.Tests
{
    public class NetworkServerTests
    {
        NetworkServer testServer;
        GameObject serverGO;

        [UnitySetUp]
        public void SetupNetworkServer()
        {
            RunAsync(async () =>
            {
                serverGO = new GameObject();
                testServer = serverGO.AddComponent<NetworkServer>();
                serverGO.AddComponent<NetworkClient>();
                await testServer.ListenAsync();

            });
        }

        [Test]
        public void InitializeTest()
        {
            Assert.That(testServer.connections.Count == 0);
            Assert.That(testServer.active);
            Assert.That(testServer.LocalClientActive, Is.False);
        }

        [Test]
        public void SpawnTest()
        {
            var gameObject = new GameObject();
            gameObject.AddComponent<NetworkIdentity>();
            testServer.Spawn(gameObject);

            Assert.That(gameObject.GetComponent<NetworkIdentity>().server == testServer);
        }


        [Test]
        public void ShutdownTest()
        {
            testServer.Shutdown();
            Assert.That(testServer.active == false);
        }

        [TearDown]
        public void ShutdownNetworkServer()
        {
            testServer.Shutdown();
            GameObject.DestroyImmediate(serverGO);
        }
    }
}
