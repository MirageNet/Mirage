using System;
using System.Collections.Generic;
using System.Reflection;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Mirage.Tests.Runtime
{
    public class NetworkClientSendIfDisconnectedTest
    {
        public GameObject clientGO;
        public NetworkClient clientNetClient;

        [SetUp]
        public void SetUp()
        {
            // Initialize...
            clientGO = new GameObject("Mirage Network Client Object", typeof(NetworkClient));
            clientNetClient = clientGO.GetComponent<NetworkClient>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(clientGO);
        }

        [Test]
        // Ensure that the created client game object and it's component(s) isn't null.
        public void EnsureNetworkClientIsNotNull()
        {
            Assert.IsNotNull(clientGO);
            Assert.IsNotNull(clientNetClient);
        }

        [Test]
        // Ensure that the client object's Player reference is null.
        public void EnsureNetworkClientPlayerIsNull()
        {
            Assert.IsNull(clientNetClient.Player);
        }

        [Test]
        // Make sure we throw IOE to prevent Send attempts.
        public void EnsureNetworkClientDoesntSendWhenDisconnected()
        {
            // Send out data.
            // This should always invoke a InvalidOperationException.
            try
            {
                clientNetClient.Send(new byte[] { 0x13, 0x37, 0x69 });
            }
            catch (Exception ex)
            {
                Assert.That(ex is InvalidOperationException);
            }
        }
    }
}
