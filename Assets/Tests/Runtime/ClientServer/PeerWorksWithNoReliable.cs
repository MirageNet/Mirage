using System.Collections;
using System.Collections.Generic;
using Mirage.SocketLayer;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.ClientServer.PeerUseTest
{
    [TestFixture(true)]
    [TestFixture(false)]
    public class PeerWorksWithNoReliable : ClientServerSetup
    {
        private readonly Config config;
        protected override Config ClientConfig => config;
        protected override Config ServerConfig => config;

        public PeerWorksWithNoReliable(bool disableReliableLayer)
        {
            config = new Config()
            {
                DisableReliableLayer = disableReliableLayer,
            };
        }

        [UnityTest]
        public IEnumerator CanReceiveMessage()
        {
            const string msg = "hello world";

            var receieved = new List<string>();
            server.MessageHandler.RegisterHandler<MyMessage>(x => receieved.Add(x.message));

            client.Send(new MyMessage
            {
                message = msg
            });
            yield return null;
            yield return null;

            Assert.That(receieved, Has.Count.EqualTo(1));
            Assert.That(receieved[0], Is.EqualTo(msg));
        }

        [UnityTest]
        public IEnumerator CanReceiveSmallMessage()
        {
            var count = 0;
            server.MessageHandler.RegisterHandler<SmallMessage>(x => count++);

            client.Send(new SmallMessage { });
            yield return null;
            yield return null;

            Assert.That(count, Is.EqualTo(1));
        }

        [NetworkMessage]
        public struct SmallMessage
        {
        }

        [NetworkMessage]
        public struct MyMessage
        {
            public string message;
        }
    }
}
