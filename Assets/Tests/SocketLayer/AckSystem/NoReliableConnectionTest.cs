using System;
using System.Collections.Generic;
using System.Linq;
using Mirage.SocketLayer.Tests.PeerTests;
using NSubstitute;
using NUnit.Framework;

namespace Mirage.SocketLayer.Tests
{
    [Category("SocketLayer")]
    public abstract class ConnectionTestBase
    {
        protected IConnection _connection;
        protected byte[] _buffer;
        protected Config _config;
        protected PeerInstance _peerInstance;
        protected Pool<ByteBuffer> _bufferPool;
        protected readonly Random rand = new Random();
        protected List<byte[]> _sentArrays = new List<byte[]>();

        protected ISocket Socket => _peerInstance.socket;
        protected abstract Config CreateConfig();
        protected virtual int MAX_PACKET_SIZE => 100;

        [SetUp]
        public void Setup()
        {
            _config = CreateConfig();
            _peerInstance = new PeerInstance(_config, maxPacketSize: MAX_PACKET_SIZE);
            _bufferPool = new Pool<ByteBuffer>(ByteBuffer.CreateNew, MAX_PACKET_SIZE, 0, 100);

            _connection = _peerInstance.peer.Connect(Substitute.For<IEndPoint>());

            _buffer = new byte[MAX_PACKET_SIZE - 1];
            for (var i = 0; i < _buffer.Length; i++)
            {
                _buffer[i] = (byte)i;
            }

            // clear calls, Connect will have sent one
            _sentArrays.Clear();
            Socket.ClearReceivedCalls();
            Socket.When(x => x.Send(Arg.Any<IEndPoint>(), Arg.Any<byte[]>(), Arg.Any<int>()))
                .Do(x =>
                {
                    var packet = (byte[])x.Args()[1];
                    var length = (int)x.Args()[2];
                    // create copy
                    _sentArrays.Add(packet.Take(length).ToArray());
                });
        }


        protected void AssertSentPacket(PacketType type, IEnumerable<int> messageLengths)
        {
            var totalLength = 1 + (2 * messageLengths.Count()) + messageLengths.Sum();

            // only 1 at any length
            Socket.Received(1).Send(Arg.Any<IEndPoint>(), Arg.Any<byte[]>(), Arg.Any<int>());
            // but also check we received length
            Socket.Received(1).Send(Arg.Any<IEndPoint>(), Arg.Any<byte[]>(), totalLength);

            // check packet was correct
            CheckMessage(type, 0, 1, messageLengths);

            // clear calls after, so we are ready to process next message
            Socket.ClearReceivedCalls();
            _sentArrays.Clear();
        }

        protected void CheckMessage(PacketType type, int sentIndex, int sendCount, IEnumerable<int> messageLengths, int skipHeader = 0)
        {
            Assert.That(_sentArrays.Count, Is.EqualTo(sendCount));
            var packet = _sentArrays[sentIndex];
            if (packet[0] != (byte)type)
                Assert.Fail($"First byte should be the packet type, {type}, it was {(PacketType)packet[0]} instead");

            var offset = 1;
            foreach (var length in messageLengths)
            {
                if (skipHeader == 0)
                {
                    var ln = ByteUtils.ReadUShort(packet, ref offset);
                    if (ln != length)
                        Assert.Fail($"Length at offset {offset - 2} was incorrect.\n  Expected:{length}\n   But war:{ln}");


                    for (var i = 0; i < length; i++)
                    {
                        if (packet[offset + i] != _buffer[i])
                            Assert.Fail($"Value at offset {offset + i} was incorrect.\n  Expected:{_buffer[i]}\n   But war:{packet[offset + i]}");

                    }
                    offset += length;
                }
                else
                {
                    offset += skipHeader;
                    for (var i = 0; i < length; i++)
                    {
                        if (packet[offset + i] != _buffer[i])
                            Assert.Fail($"Value at offset {offset + i} was incorrect.\n  Expected:{_buffer[i]}\n   But war:{packet[offset + i]}");

                    }
                    offset += length;
                }
            }

            Assert.That(offset, Is.EqualTo(packet.Length));
        }

        protected void SendIntoBatch(int length, bool reliable, ref int total, List<int> currentBatch)
        {
            // will write length+2
            var newTotal = total + 2 + length;
            if (newTotal > MAX_PACKET_SIZE)
            {
                Send(reliable, _buffer, length);
                // was over max, so should have sent
                AssertSentPacket(reliable ? PacketType.Reliable : PacketType.Unreliable, currentBatch);

                currentBatch.Clear();
                // new batch
                total = 1 + 2 + length;
            }
            else
            {
                Send(reliable, _buffer, length);
                Socket.DidNotReceive().Send(Arg.Any<IEndPoint>(), Arg.Any<byte[]>(), Arg.Any<int>());
                total = newTotal;
            }
            currentBatch.Add(length);
        }

        protected void Send(bool reliable, byte[] buffer, int length)
        {
            if (reliable)
                _connection.SendReliable(buffer, 0, length);
            else
                _connection.SendUnreliable(buffer, 0, length);
        }
    }

    [Category("SocketLayer")]
    public class NoReliableConnectionTest : ConnectionTestBase
    {
        private new Connection _connection => (Connection)base._connection;

        protected override Config CreateConfig()
        {
            return new Config
            {
                DisableReliableLayer = true,
            };
        }

        [Test]
        public void IsNoReliableConnection()
        {
            Assert.That(_connection, Is.TypeOf<NoReliableConnection>());
        }

        [Test]
        public void ThrowsIfTooBig()
        {
            // 3 byte header, so max size is over max
            var bigBuffer = new byte[MAX_PACKET_SIZE - 2];

            var exception = Assert.Throws<ArgumentException>(() =>
            {
                _connection.SendReliable(bigBuffer);
            });

            var expected = new ArgumentException($"Message is bigger than MTU, size:{bigBuffer.Length} but max message size is {MAX_PACKET_SIZE - 3}");
            Assert.That(exception, Has.Message.EqualTo(expected.Message));
        }

        [Test]
        public void MessageAreBatched()
        {
            // max is 100

            var lessThanBatchLengths = new int[]
            {
                20, 40, 30
            };
            var overBatch = 11;

            foreach (var length in lessThanBatchLengths)
            {
                _connection.SendReliable(_buffer, 0, length);
                Socket.DidNotReceive().Send(Arg.Any<IEndPoint>(), Arg.Any<byte[]>(), Arg.Any<int>());
            }

            // should be 97 in buffer now => 1+(length+2)*3
            _connection.SendReliable(_buffer, 0, overBatch);
            AssertSentPacket(PacketType.Reliable, lessThanBatchLengths);
        }

        [Test]
        [Repeat(100)]
        public void MessageAreBatched_Repeat()
        {
            const int messageCount = 10;
            var lengths = new int[messageCount];
            for (var i = 0; i < messageCount; i++)
            {
                lengths[i] = rand.Next(10, MAX_PACKET_SIZE - 3);
            }
            var currentBatch = new List<int>();

            var total = 1;
            foreach (var length in lengths)
            {
                // will write length+2
                var newTotal = total + 2 + length;
                if (newTotal > MAX_PACKET_SIZE)
                {
                    _connection.SendReliable(_buffer, 0, length);
                    // was over max, so should have sent
                    AssertSentPacket(PacketType.Reliable, currentBatch);

                    currentBatch.Clear();
                    // new batch
                    total = 1 + 2 + length;
                }
                else
                {
                    _connection.SendReliable(_buffer, 0, length);
                    Socket.DidNotReceive().Send(Arg.Any<IEndPoint>(), Arg.Any<byte[]>(), Arg.Any<int>());
                    total = newTotal;
                }
                currentBatch.Add(length);
            }
        }

        [Test]
        public void FlushSendsMessageInBatch()
        {
            // max is 100

            var lessThanBatchLengths = new int[]
            {
                20, 40
            };

            foreach (var length in lessThanBatchLengths)
            {
                _connection.SendReliable(_buffer, 0, length);
                Socket.DidNotReceive().Send(Arg.Any<IEndPoint>(), Arg.Any<byte[]>(), Arg.Any<int>());
            }

            _connection.FlushBatch();
            AssertSentPacket(PacketType.Reliable, lessThanBatchLengths);
        }

        [Test]
        public void FlushDoesNotSendEmptyMessage()
        {
            _connection.FlushBatch();
            Socket.DidNotReceive().Send(Arg.Any<IEndPoint>(), Arg.Any<byte[]>(), Arg.Any<int>());
            _connection.FlushBatch();
            Socket.DidNotReceive().Send(Arg.Any<IEndPoint>(), Arg.Any<byte[]>(), Arg.Any<int>());
        }


        [Test]
        public void UnbatchesMessageOnReceive()
        {
            var receive = _bufferPool.Take();
            receive.array[0] = (byte)PacketType.Reliable;
            var offset = 1;
            AddMessage(receive.array, ref offset, 10);
            AddMessage(receive.array, ref offset, 30);
            AddMessage(receive.array, ref offset, 20);

            var segments = new List<ArraySegment<byte>>();
            _peerInstance.dataHandler
                .When(x => x.ReceiveMessage(_connection, Arg.Any<ArraySegment<byte>>()))
                .Do(x => segments.Add(x.ArgAt<ArraySegment<byte>>(1)));
            ((NoReliableConnection)_connection).ReceiveReliablePacket(new Packet(receive, offset));
            _peerInstance.dataHandler.Received(3).ReceiveMessage(_connection, Arg.Any<ArraySegment<byte>>());


            Assert.That(segments[0].Count, Is.EqualTo(10));
            Assert.That(segments[1].Count, Is.EqualTo(30));
            Assert.That(segments[2].Count, Is.EqualTo(20));
            Assert.That(segments[0].SequenceEqual(new ArraySegment<byte>(_buffer, 0, 10)));
            Assert.That(segments[1].SequenceEqual(new ArraySegment<byte>(_buffer, 0, 30)));
            Assert.That(segments[2].SequenceEqual(new ArraySegment<byte>(_buffer, 0, 20)));
        }

        private void AddMessage(byte[] receive, ref int offset, int size)
        {
            ByteUtils.WriteUShort(receive, ref offset, (ushort)size);
            Buffer.BlockCopy(_buffer, 0, receive, offset, size);
            offset += size;
        }

        [Test]
        public void SendingToUnreliableUsesReliable()
        {
            var counts = new List<int>() { 10, 20 };
            _connection.SendUnreliable(_buffer, 0, counts[0]);
            _connection.SendUnreliable(_buffer, 0, counts[1]);
            _connection.FlushBatch();

            AssertSentPacket(PacketType.Reliable, counts);
        }

        [Test]
        public void SendingToNotifyUsesReliable()
        {
            var counts = new List<int>() { 10, 20 };
            _connection.SendNotify(_buffer, 0, counts[0]);
            _connection.SendNotify(_buffer, 0, counts[1]);
            _connection.FlushBatch();

            AssertSentPacket(PacketType.Reliable, counts);
        }
        [Test]
        public void SendingToNotifyTokenUsesReliable()
        {
            var token = Substitute.For<INotifyCallBack>();
            var counts = new List<int>() { 10, 20 };
            _connection.SendNotify(_buffer, 0, counts[0], token);
            _connection.SendNotify(_buffer, 0, counts[1], token);
            _connection.FlushBatch();

            AssertSentPacket(PacketType.Reliable, counts);
        }

        [Test]
        public void NotifyOnDeliveredInvoke()
        {
            var counts = new List<int>() { 10, 20 };
            var token = _connection.SendNotify(_buffer, 0, counts[0]);
            Assert.That(token, Is.TypeOf<AutoCompleteToken>());

            var action = Substitute.For<Action>();
            token.Delivered += action;
            action.Received(1).Invoke();
        }

        [Test]
        public void NotifyTokenOnDeliveredInvoke()
        {
            var token = Substitute.For<INotifyCallBack>();
            var counts = new List<int>() { 10, 20 };
            _connection.SendNotify(_buffer, 0, counts[0], token);
            token.Received(1).OnDelivered();
        }
    }



    [Category("SocketLayer")]
    public class LargeMessageOnTest : ConnectionTestBase
    {
        private new Connection _connection => (Connection)base._connection;
        protected override int MAX_PACKET_SIZE => ushort.MaxValue + 5000;

        protected override Config CreateConfig()
        {
            return new Config
            {
                DisableReliableLayer = true,
            };
        }

        [Test]
        public void ThrowsIfTooBig()
        {
            // 3 byte header, so max size is over max
            var bigBuffer = new byte[MAX_PACKET_SIZE - 2];

            var exception = Assert.Throws<ArgumentException>(() =>
            {
                _connection.SendReliable(bigBuffer);
            });

            var expected = new ArgumentException($"Message is bigger than MTU, size:{bigBuffer.Length} but max message size is {MAX_PACKET_SIZE - 3}");
            Assert.That(exception, Has.Message.EqualTo(expected.Message));
        }

        [Test]
        public void MessageOverUshortAreNotBatched()
        {
            var length = ushort.MaxValue + 10;

            _connection.SendReliable(_buffer, 0, length);

            var totalLength = 1 + 2 + length;
            Socket.Received(1).Send(Arg.Any<IEndPoint>(), Arg.Any<byte[]>(), totalLength);

            // check packet was correct
            Assert.That(_sentArrays.Count, Is.EqualTo(1));
            var packet = _sentArrays[0];
            Assert.That(packet.Length, Is.EqualTo(totalLength));
            if (packet[0] != (byte)PacketType.Reliable)
                Assert.Fail($"First byte should be the packet type, {PacketType.Reliable}, it was {(PacketType)packet[0]} instead");

            var offset = 1;
            var ln = ByteUtils.ReadUShort(packet, ref offset);
            Assert.That(ln, Is.EqualTo(0), "non-batch message should have length zero");
            for (var i = 0; i < length; i++)
            {
                if (packet[offset + i] != _buffer[i])
                    Assert.Fail($"Value at offset {offset + i} was incorrect.\n  Expected:{_buffer[i]}\n   But war:{packet[offset + i]}");
            }
            offset += length;

            Assert.That(offset, Is.EqualTo(packet.Length));
        }

        [Test]
        [Repeat(100)]
        public void MessageAreBatched_Repeat()
        {
            const int messageCount = 10;
            var lengths = new int[messageCount];
            for (var i = 0; i < messageCount; i++)
            {
                lengths[i] = rand.Next(10, MAX_PACKET_SIZE - 3);
            }
            var currentBatch = new List<int>();

            var total = 1;
            foreach (var length in lengths)
            {
                // will write length+2
                var newTotal = total + 2 + length;
                if (newTotal > MAX_PACKET_SIZE)
                {
                    _connection.SendReliable(_buffer, 0, length);
                    // was over max, so should have sent
                    AssertSentPacket(PacketType.Reliable, currentBatch);

                    currentBatch.Clear();
                    // new batch
                    total = 1 + 2 + length;
                }
                else
                {
                    _connection.SendReliable(_buffer, 0, length);
                    Socket.DidNotReceive().Send(Arg.Any<IEndPoint>(), Arg.Any<byte[]>(), Arg.Any<int>());
                    total = newTotal;
                }
                currentBatch.Add(length);
            }
        }

        [Test]
        public void FlushSendsMessageInBatch()
        {
            // max is 100

            var lessThanBatchLengths = new int[]
            {
                20, 40
            };

            foreach (var length in lessThanBatchLengths)
            {
                _connection.SendReliable(_buffer, 0, length);
                Socket.DidNotReceive().Send(Arg.Any<IEndPoint>(), Arg.Any<byte[]>(), Arg.Any<int>());
            }

            _connection.FlushBatch();
            AssertSentPacket(PacketType.Reliable, lessThanBatchLengths);
        }

        [Test]
        public void FlushDoesNotSendEmptyMessage()
        {
            _connection.FlushBatch();
            Socket.DidNotReceive().Send(Arg.Any<IEndPoint>(), Arg.Any<byte[]>(), Arg.Any<int>());
            _connection.FlushBatch();
            Socket.DidNotReceive().Send(Arg.Any<IEndPoint>(), Arg.Any<byte[]>(), Arg.Any<int>());
        }


        [Test]
        public void UnbatchesMessageOnReceive()
        {
            var receive = _bufferPool.Take();
            receive.array[0] = (byte)PacketType.Reliable;
            var offset = 1;
            AddMessage(receive.array, ref offset, 10);
            AddMessage(receive.array, ref offset, 30);
            AddMessage(receive.array, ref offset, 20);

            var segments = new List<ArraySegment<byte>>();
            _peerInstance.dataHandler
                .When(x => x.ReceiveMessage(_connection, Arg.Any<ArraySegment<byte>>()))
                .Do(x => segments.Add(x.ArgAt<ArraySegment<byte>>(1)));
            ((NoReliableConnection)_connection).ReceiveReliablePacket(new Packet(receive, offset));
            _peerInstance.dataHandler.Received(3).ReceiveMessage(_connection, Arg.Any<ArraySegment<byte>>());


            Assert.That(segments[0].Count, Is.EqualTo(10));
            Assert.That(segments[1].Count, Is.EqualTo(30));
            Assert.That(segments[2].Count, Is.EqualTo(20));
            Assert.That(segments[0].SequenceEqual(new ArraySegment<byte>(_buffer, 0, 10)));
            Assert.That(segments[1].SequenceEqual(new ArraySegment<byte>(_buffer, 0, 30)));
            Assert.That(segments[2].SequenceEqual(new ArraySegment<byte>(_buffer, 0, 20)));
        }

        private void AddMessage(byte[] receive, ref int offset, int size)
        {
            ByteUtils.WriteUShort(receive, ref offset, (ushort)size);
            Buffer.BlockCopy(_buffer, 0, receive, offset, size);
            offset += size;
        }

        [Test]
        public void SendingToUnreliableUsesReliable()
        {
            var counts = new List<int>() { 10, 20 };
            _connection.SendUnreliable(_buffer, 0, counts[0]);
            _connection.SendUnreliable(_buffer, 0, counts[1]);
            _connection.FlushBatch();

            AssertSentPacket(PacketType.Reliable, counts);
        }

        [Test]
        public void SendingToNotifyUsesReliable()
        {
            var counts = new List<int>() { 10, 20 };
            _connection.SendNotify(_buffer, 0, counts[0]);
            _connection.SendNotify(_buffer, 0, counts[1]);
            _connection.FlushBatch();

            AssertSentPacket(PacketType.Reliable, counts);
        }
        [Test]
        public void SendingToNotifyTokenUsesReliable()
        {
            var token = Substitute.For<INotifyCallBack>();
            var counts = new List<int>() { 10, 20 };
            _connection.SendNotify(_buffer, 0, counts[0], token);
            _connection.SendNotify(_buffer, 0, counts[1], token);
            _connection.FlushBatch();

            AssertSentPacket(PacketType.Reliable, counts);
        }

        [Test]
        public void NotifyOnDeliveredInvoke()
        {
            var counts = new List<int>() { 10, 20 };
            var token = _connection.SendNotify(_buffer, 0, counts[0]);
            Assert.That(token, Is.TypeOf<AutoCompleteToken>());

            var action = Substitute.For<Action>();
            token.Delivered += action;
            action.Received(1).Invoke();
        }

        [Test]
        public void NotifyTokenOnDeliveredInvoke()
        {
            var token = Substitute.For<INotifyCallBack>();
            var counts = new List<int>() { 10, 20 };
            _connection.SendNotify(_buffer, 0, counts[0], token);
            token.Received(1).OnDelivered();
        }
    }
}
