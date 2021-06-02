using System;
using NUnit.Framework;

namespace Mirage.Tests.Runtime.Host
{
    public class NetworkServerNoAutoStartTest : HostSetup<MockComponent>
    {
        protected override bool AutoStartServer => false;

        [Test]
        public void InvokeLocalConnectedThrowsIfLocalClientIsNull()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                server.InvokeLocalConnected();
            });
        }
    }
}
