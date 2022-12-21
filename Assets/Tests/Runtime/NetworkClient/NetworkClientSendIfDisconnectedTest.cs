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

            // Ensure that these are correct for this test.
            // - The NetworkClient reference is not null.
            // - The NetworkClient Player reference must be null.
            Debug.Assert(clientNetClient != null);
            Debug.Assert(clientNetClient.Player == null);           
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(clientGO);
        }

        [Test]
        // Make sure we throw IOE to prevent Send attempts.
        public void EnsureNetworkClientDoesntSendWhenDisconnected()
        {
            // Send out data.
            // This should always invoke a InvalidOperationException.
            Assert.Throws<InvalidOperationException>(() => clientNetClient.Send(new byte[] { 0x13, 0x37, 0x69 }));
        }
    }
}
