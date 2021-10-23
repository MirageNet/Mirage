using System;
using Mirage;
using NUnit.Framework;
using UnityEngine;

namespace Mirage.Tests.Runtime.Components
{
    public class PingDisplayTest
    {
        [Test]
        public void PingDisplayClientNullReferencesUpdateTest()
        {
            var gameObject = new GameObject("pingDisplay", typeof(NetworkPingDisplay));

            NetworkPingDisplay pingDisplay = gameObject.GetComponent<NetworkPingDisplay>();

            NullReferenceException test = Assert.Throws<NullReferenceException>(() => 
            {
                pingDisplay.Update();
            });
        }

        [Test]
        public void PingDisplayTextLabelNullReferencesUpdateTest()
        {
            var gameObject = new GameObject("pingDisplay", typeof(NetworkPingDisplay), typeof(NetworkClient));

            NetworkPingDisplay pingDisplay = gameObject.GetComponent<NetworkPingDisplay>();
            pingDisplay.Client = gameObject.GetComponent<NetworkClient>();
            pingDisplay.Client.connectState = ConnectState.Connected;

            NullReferenceException test = Assert.Throws<NullReferenceException>(() =>
            {
                pingDisplay.Update();
            });
        }
    }
}
