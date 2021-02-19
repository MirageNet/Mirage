using System;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Mirage.Tests.Host
{

    [TestFixture]
    public class NetworkServerTest : HostSetup<MockComponent>
    {
        [Test]
        public void MaxConnectionsTest()
        {
            var secondGO = new GameObject();
            NetworkClient secondClient = secondGO.AddComponent<NetworkClient>();
            Transport secondTestTransport = secondGO.AddComponent<LoopbackTransport>();

            secondClient.Transport = secondTestTransport;

            secondClient.ConnectAsync("localhost").Forget();

            Assert.That(server.connections, Has.Count.EqualTo(1));

            Object.Destroy(secondGO);
        }

        [Test]
        public void LocalClientActiveTest()
        {
            Assert.That(server.LocalClientActive, Is.True);
        }

        [Test]
        public void SetLocalConnectionExceptionTest()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                server.SetLocalConnection(null, null);
            });
        }

        [Test]
        public void StartedNotNullTest()
        {
            Assert.That(server.Started, Is.Not.Null);
        }

        [Test]
        public void ConnectedNotNullTest()
        {
            Assert.That(server.Connected, Is.Not.Null);
        }

        [Test]
        public void AuthenticatedNotNullTest()
        {
            Assert.That(server.Authenticated, Is.Not.Null);
        }

        [Test]
        public void DisconnectedNotNullTest()
        {
            Assert.That(server.Disconnected, Is.Not.Null);
        }

        [Test]
        public void StoppedNotNullTest()
        {
            Assert.That(server.Stopped, Is.Not.Null);
        }

        [Test]
        public void OnStartHostNotNullTest()
        {
            Assert.That(server.OnStartHost, Is.Not.Null);
        }

        [Test]
        public void OnStopHostNotNullTest()
        {
            Assert.That(server.OnStopHost, Is.Not.Null);
        }
    }
}
