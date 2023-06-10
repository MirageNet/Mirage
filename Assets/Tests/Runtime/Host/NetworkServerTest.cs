using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Mirage.Tests.Runtime.Host
{
    [TestFixture]
    public class NetworkServerTest : HostSetup
    {
        private readonly List<INetworkPlayer> serverConnectedCalls = new List<INetworkPlayer>();
        private readonly List<INetworkPlayer> clientConnectedCalls = new List<INetworkPlayer>();

        protected override void ExtraServerSetup()
        {
            base.ExtraServerSetup();

            serverConnectedCalls.Clear();
            clientConnectedCalls.Clear();

            server.Connected.AddListener(player => serverConnectedCalls.Add(player));
            client.Connected.AddListener(player => clientConnectedCalls.Add(player));
        }

        [Test]
        public void ConnectedEventIsCalledOnceForServer()
        {
            Assert.That(serverConnectedCalls, Has.Count.EqualTo(1));
            Assert.That(serverConnectedCalls[0].Connection, Is.TypeOf<PipePeerConnection>());
        }
        [Test]
        public void ConnectedEventIsCalledOnceForClient()
        {
            Assert.That(clientConnectedCalls, Has.Count.EqualTo(1));
            Assert.That(clientConnectedCalls[0].Connection, Is.TypeOf<PipePeerConnection>());
        }


        [Test]
        public void LocalClientActiveTest()
        {
            Assert.That(server.LocalClientActive, Is.True);
        }

        [Test]
        public void AddLocalConnectionExceptionTest()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                server.AddLocalConnection(null, null);
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

        [Test]
        public void TimeNotNullTest()
        {
            Assert.That(server.World.Time, Is.Not.Null);
        }
    }
}
