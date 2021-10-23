using System;
using Mirage.Tests.Runtime.Host;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Mirage.Tests.Runtime.Components
{
    public class PingDisplayTest : HostSetup<MockComponent>
    {
        [Test]
        public void PingDisplayTextLabelNullReferencesUpdateTest()
        {
            var gameObject = new GameObject("pingDisplay", typeof(NetworkPingDisplay));

            NetworkPingDisplay pingDisplay = gameObject.GetComponent<NetworkPingDisplay>();
            pingDisplay.Client = client;

            NullReferenceException nullException = Assert.Throws<NullReferenceException>(() =>
            {
                pingDisplay.Update();
            });
        }

        [Test]

        public void PingDisplayTextChangedValue()
        {
            var gameObject = new GameObject("pingDisplay", typeof(NetworkPingDisplay), typeof(Text));

            NetworkPingDisplay pingDisplay = gameObject.GetComponent<NetworkPingDisplay>();
            pingDisplay.Client = client;
            pingDisplay.NetworkPingLabelText = gameObject.GetComponent<Text>();

            Text oldValue = pingDisplay.NetworkPingLabelText;

            pingDisplay.Update();

            Assert.That(oldValue != pingDisplay.NetworkPingLabelText, Is.False);
        }
    }
}
