using Mirage.SocketLayer;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Mirage.Tests.Runtime.Host
{
    public class NetworkServerMaxConnectionTests : HostSetup<MockComponent>
    {
        protected override Config ServerConfig => new Config
        {
            // max is 0 becuase host connection isn't included for peer
            MaxConnections = 0
        };

        [Test]
        public void DisconnectsConnectionsOverMax()
        {
            var secondGO = new GameObject();
            var secondClient = secondGO.AddComponent<NetworkClient>();
            var socketFactory = networkManagerGo.GetComponent<TestSocketFactory>();

            secondClient.SocketFactory = socketFactory;

            var disconnectedCalled = 0;
            secondClient.Disconnected.AddListener(reason =>
            {
                disconnectedCalled++;
                Assert.That(reason, Is.EqualTo(ClientStoppedReason.ServerFull));
            });
            secondClient.Connect("localhost");

            // updates both so that connect and reject message is received 
            DoUpdate();
            secondClient.Update();

            Assert.That(server.Players, Has.Count.EqualTo(1));
            // also check if client was disconnected (this will confirm it was rejected
            Assert.That(disconnectedCalled, Is.EqualTo(1));

            Object.Destroy(secondGO);
        }
    }
}
