using System;
using NSubstitute;
using NUnit.Framework;

namespace Mirage.SocketLayer.Tests.PeerTests
{
    [Category("SocketLayer")]
    public class PeerMessageSizeTest
    {
        private const int MAX_PACKET_SIZE = 1200;

        [Test]
        public void GetMaxUnreliableMessageSize_NoReliable()
        {
            var config = new Config { DisableReliableLayer = true };
            var peer = new Peer(Substitute.For<ISocket>(), MAX_PACKET_SIZE, Substitute.For<IDataHandler>(), config);

            // header is 1 byte for type, 2 for length
            const int header = 1 + 2;
            var expected = MAX_PACKET_SIZE - header;
            Assert.That(peer.GetMaxUnreliableMessageSize(), Is.EqualTo(expected));
        }

        [Test]
        public void GetMaxUnreliableMessageSize_WithReliable()
        {
            var config = new Config { DisableReliableLayer = false };
            var peer = new Peer(Substitute.For<ISocket>(), MAX_PACKET_SIZE, Substitute.For<IDataHandler>(), config);

            // header is 1 byte for type, 2 for length
            const int header = 1 + 2;
            var expected = MAX_PACKET_SIZE - header;
            Assert.That(peer.GetMaxUnreliableMessageSize(), Is.EqualTo(expected));
        }

        [Test]
        public void GetMaxNotifyMessageSize_NoReliable()
        {
            var config = new Config { DisableReliableLayer = true };
            var peer = new Peer(Substitute.For<ISocket>(), MAX_PACKET_SIZE, Substitute.For<IDataHandler>(), config);

            // NoReliableConnection calls SendReliable for notify
            const int header = 1 + 2; // packet type + message length
            var expected = MAX_PACKET_SIZE - header;
            Assert.That(peer.GetMaxNotifyMessageSize(), Is.EqualTo(expected));
        }

        [Test]
        public void GetMaxNotifyMessageSize_WithReliable()
        {
            var config = new Config { DisableReliableLayer = false };
            var peer = new Peer(Substitute.For<ISocket>(), MAX_PACKET_SIZE, Substitute.For<IDataHandler>(), config);

            // from AckSystem
            // PacketType, sequence, ack sequence, mask
            const int notifyHeader = 1 + 2 + 2 + 8;
            var expected = MAX_PACKET_SIZE - notifyHeader;
            Assert.That(peer.GetMaxNotifyMessageSize(), Is.EqualTo(expected));
        }

        [Test]
        public void GetMaxReliableMessageSize_NoReliable()
        {
            var config = new Config { DisableReliableLayer = true };
            var peer = new Peer(Substitute.For<ISocket>(), MAX_PACKET_SIZE, Substitute.For<IDataHandler>(), config);

            // from NoReliableConnection
            const int header = 1 + 2; // packet type + message length
            var expected = MAX_PACKET_SIZE - header;
            Assert.That(peer.GetMaxReliableMessageSize(), Is.EqualTo(expected));
        }

        [Test]
        public void GetMaxReliableMessageSize_WithReliable_NoFragment()
        {
            var config = new Config { DisableReliableLayer = false, MaxReliableFragments = -1 };
            var peer = new Peer(Substitute.For<ISocket>(), MAX_PACKET_SIZE, Substitute.For<IDataHandler>(), config);

            // if not fragmented
            // PacketType, sequence, ack sequence, mask, order
            const int reliableHeader = 1 + 2 + 2 + 8 + 2;
            const int messageLengthSize = 2;
            const int minReliableHeader = reliableHeader + messageLengthSize;
            var expected = MAX_PACKET_SIZE - minReliableHeader;
            Assert.That(peer.GetMaxReliableMessageSize(), Is.EqualTo(expected));
        }


        [TestCase(1)]
        [TestCase(8)]
        [TestCase(63)]
        [TestCase(255)]
        public void GetMaxReliableMessageSize_WithReliable_WithFragment(int maxFragments)
        {
            var config = new Config { DisableReliableLayer = false, MaxReliableFragments = maxFragments };
            var peer = new Peer(Substitute.For<ISocket>(), MAX_PACKET_SIZE, Substitute.For<IDataHandler>(), config);

            // PacketType, sequence, ack sequence, mask, order
            const int reliableHeader = 1 + 2 + 2 + 8 + 2;
            const int fragmentIndexSize = 1;
            const int minReliableFragmentHeader = reliableHeader + fragmentIndexSize;
            var sizePerFragment = MAX_PACKET_SIZE - minReliableFragmentHeader;
            var expected = maxFragments * sizePerFragment;
            Assert.That(peer.GetMaxReliableMessageSize(), Is.EqualTo(expected));
        }

        [Test]
        public void PeerConstructor_ThrowsIfMaxFragmentsIsTooHigh()
        {
            var config = new Config
            {
                MaxReliableFragments = 256
            };

            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                new Peer(Substitute.For<ISocket>(), MAX_PACKET_SIZE, Substitute.For<IDataHandler>(), config);
            });

            Assert.That(exception.ParamName, Is.EqualTo("MaxReliableFragments"));
        }

        [Test]
        public void PeerConstructor_DoesNotThrowIfMaxFragmentsIsAtLimit()
        {
            var config = new Config
            {
                MaxReliableFragments = 255
            };

            Assert.DoesNotThrow(() =>
            {
                new Peer(Substitute.For<ISocket>(), MAX_PACKET_SIZE, Substitute.For<IDataHandler>(), config);
            });
        }
    }
}
