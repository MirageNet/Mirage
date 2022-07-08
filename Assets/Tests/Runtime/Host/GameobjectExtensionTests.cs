using System;
using NUnit.Framework;
using UnityEngine;

namespace Mirage.Tests.Runtime.Host
{
    public class GameobjectExtensionTests : HostSetup<MockComponent>
    {
        [Test]
        public void GetNetworkIdentity()
        {
            Assert.That(playerGO.GetNetworkIdentity(), Is.EqualTo(playerIdentity));
        }

        [Test]
        public void GetNoNetworkIdentity()
        {
            // create a GameObject without NetworkIdentity
            GameObject goWithout = CreateGameObject();

            // GetNetworkIdentity for GO without identity
            // (error log is expected)
            Assert.Throws<InvalidOperationException>(() =>
            {
                _ = goWithout.GetNetworkIdentity();
            });
        }
    }
}
