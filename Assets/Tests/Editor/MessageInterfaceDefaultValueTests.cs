using System;
using NSubstitute;
using NUnit.Framework;

namespace Mirage
{
    public class MessageInterfaceDefaultValueTests
    {
        [Test]
        public void SenderUsesDefaultChannelForMessage()
        {
            IMessageSender sender = Substitute.For<IMessageSender>();
            var msg = new NetworkPingMessage();
            sender.Send(msg);
            sender.Received(1).Send(msg, Channel.Reliable);
        }

        [Test]
        public void SenderUsesDefaultChannelForSegement()
        {
            IMessageSender sender = Substitute.For<IMessageSender>();
            var segment = new ArraySegment<byte>();
            sender.Send(segment);
            sender.Received(1).Send(segment, Channel.Reliable);
        }

        [Test]
        public void NotifySenderUsesDefaultChannelForSegement()
        {
            INotifySender sender = Substitute.For<INotifySender>();
            var msg = new NetworkPingMessage();
            object token = new object();
            sender.SendNotify(msg, token);
            sender.Received(1).SendNotify(msg, token, Channel.Unreliable);
        }
    }
}
