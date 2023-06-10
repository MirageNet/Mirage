using System.Collections;
using Mirage.SocketLayer;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.Host
{
    public class NetworkServerMaxConnectionTests : HostSetup
    {
        protected override Config ServerConfig => new Config
        {
            // max is 0 becuase host connection isn't included for peer
            MaxConnections = 0
        };

        [UnityTest]
        public IEnumerator DisconnectsConnectionsOverMax()
        {
            var secondGO = CreateGameObject();
            var secondClient = secondGO.AddComponent<NetworkClient>();
            var socketFactory = serverGo.GetComponent<TestSocketFactory>();

            secondClient.SocketFactory = socketFactory;

            var disconnectedCalled = 0;
            secondClient.Disconnected.AddListener(reason =>
            {
                disconnectedCalled++;
                Assert.That(reason, Is.EqualTo(ClientStoppedReason.ServerFull));
            });
            secondClient.Connect("localhost");

            // updates both so that connect and reject message is received 
            yield return null;
            secondClient.Update();

            Assert.That(server.Players, Has.Count.EqualTo(1));
            // also check if client was disconnected (this will confirm it was rejected
            Assert.That(disconnectedCalled, Is.EqualTo(1));
        }
    }
}
