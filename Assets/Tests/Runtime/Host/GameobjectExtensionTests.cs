using System;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Mirage.Tests.Runtime.Host
{
    public class GameobjectExtensionTests : HostSetup<MockComponent>
    {
        [Test]
        public void GetNetworkIdentity()
        {
            Assert.That(playerGO.GetNetworkIdentity(), Is.EqualTo(identity));
        }

        [Test]
        public void GetNoNetworkIdentity()
        {
            // create a GameObject without NetworkIdentity
            var goWithout = new GameObject();

            // GetNetworkIdentity for GO without identity
            // (error log is expected)
            Assert.Throws<InvalidOperationException>(() =>
            {
                _ = goWithout.GetNetworkIdentity();
            });

            // clean up
            Object.Destroy(goWithout);
        }

    }
}
