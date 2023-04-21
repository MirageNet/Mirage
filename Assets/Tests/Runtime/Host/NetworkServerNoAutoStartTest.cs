using System;
using NUnit.Framework;

namespace Mirage.Tests.Runtime.Host
{
    public class NetworkServerNoAutoStartTest : HostSetup<MockComponent>
    {
        protected override bool StartServer => false;

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
