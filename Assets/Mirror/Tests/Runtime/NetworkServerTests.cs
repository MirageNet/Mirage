using System.Collections;
using Mirror.AsyncTcp;
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
        public IEnumerator SetupNetworkServer()
        {
            return RunAsync(async () =>
            {
                serverGO = new GameObject();
                serverGO.AddComponent<AsyncTcpTransport>();
                testServer = serverGO.AddComponent<NetworkServer>();
                serverGO.AddComponent<NetworkClient>();
                await testServer.ListenAsync();

            });
        }

        [Test]
        public void InitializeTest()
        {
            Assert.That(testServer.connections, Is.Empty);
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
