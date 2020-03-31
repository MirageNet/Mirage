using System.Collections;
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
        MockTransport transport;

        [UnitySetUp]
        public IEnumerator SetupNetworkServer()
        {
            return RunAsync(async () =>
            {
                serverGO = new GameObject();
                transport = serverGO.AddComponent<MockTransport>();
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


        [TearDown]
        public void ShutdownNetworkServer()
        {
            testServer.Disconnect();
            GameObject.DestroyImmediate(serverGO);
        }
    }
}
