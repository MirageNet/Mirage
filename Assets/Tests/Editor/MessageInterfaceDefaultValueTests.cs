using System;
using NSubstitute;
using NUnit.Framework;

namespace Mirage.Tests
{
    public class MessageInterfaceDefaultValueTests
    {
        [Test]
        public void SenderUsesDefaultChannelForMessage()
        {
            var sender = Substitute.For<IMessageSender>();
            var msg = new NetworkPingMessage();
            sender.Send(msg);
            sender.Received(1).Send(msg, Channel.Reliable);
        }

        [Test]
        public void SenderUsesDefaultChannelForSegement()
        {
            var sender = Substitute.For<IMessageSender>();
            var segment = new ArraySegment<byte>();
            sender.Send(segment);
            sender.Received(1).Send(segment, Channel.Reliable);
        }
    }
}
