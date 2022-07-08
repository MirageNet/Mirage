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
            NetworkPingDisplay pingDisplay = CreateMonoBehaviour<NetworkPingDisplay>();
            pingDisplay.Client = client;

            Assert.Throws<NullReferenceException>(() =>
            {
                pingDisplay.Update();
            });
        }

        [Test]
        public void PingDisplayTextChangedValue()
        {
            GameObject gameObject = CreateGameObject();

            var pingDisplay = gameObject.AddComponent<NetworkPingDisplay>();
            pingDisplay.Client = client;
            pingDisplay.NetworkPingLabelText = gameObject.AddComponent<Text>();

            var oldValue = pingDisplay.NetworkPingLabelText;

            pingDisplay.Update();

            Assert.That(oldValue != pingDisplay.NetworkPingLabelText, Is.False);
        }
    }
}
