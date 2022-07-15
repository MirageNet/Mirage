using Mirage.Sockets.Udp;
using NUnit.Framework;
using UnityEngine;

namespace Mirage.Tests
{
    public class NetworkMenuTest
    {
        [Test]
        public void NetworkMenuTestSimplePasses()
        {
            var go = NetworkMenu.CreateNetworkManager();

            Assert.That(go.GetComponent<NetworkManager>(), Is.Not.Null);
            Assert.That(go.GetComponent<NetworkServer>(), Is.Not.Null);
            Assert.That(go.GetComponent<NetworkClient>(), Is.Not.Null);
            Assert.That(go.GetComponent<UdpSocketFactory>(), Is.Not.Null);
            Assert.That(go.GetComponent<NetworkSceneManager>(), Is.Not.Null);

            Object.DestroyImmediate(go);
        }
    }
}
