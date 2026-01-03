using System;
using System.Collections.Generic;
using System.Linq;
using Mirage.SocketLayer.Tests.PeerTests;
using Mirage.Tests;
using NSubstitute;
using NUnit.Framework;

namespace Mirage.SocketLayer.Tests.AckSystemTests
{
    [Category("SocketLayer")]
    public class NoReliableConnectionTest
    {
        private const int MAX_PACKET_SIZE = 100;

        private IConnection _connection;
        private byte[] _buffer;
        private Config _config;
        private PeerInstance _peerInstance;
        private Pool<ByteBuffer> _bufferPool;
        private readonly Random rand = new Random();

        private ISocket Socket => _peerInstance.socket;

        [SetUp]
        public void Setup()
        {
            _config = new Config
            {
                DisableReliableLayer = true,
            };
            _peerInstance = new PeerInstance(SocketBehavior.PollReceive, _config, maxPacketSize: MAX_PACKET_SIZE);
            _bufferPool = new Pool<ByteBuffer>(ByteBuffer.CreateNew, MAX_PACKET_SIZE, 0, 100);

            _connection = _peerInstance.peer.Connect((IConnectEndPoint)TestEndPoint.CreateSubstitute());
            // Set connection state to Connected after creation
            ((NoReliableConnection)_connection).State = ConnectionState.Connected;

            _buffer = new byte[MAX_PACKET_SIZE - 1];
            for (var i = 0; i < _buffer.Length; i++)
            {
                _buffer[i] = (byte)i;
            }

            // clear calls, Connect will have sent one
            Socket.ClearSendAndReceivedCalls();
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

            var exception = Assert.Throws<MessageSizeException>(() =>
            {
                _connection.SendReliable(bigBuffer);
            });

            var expected = new ArgumentException($"Message is bigger than MTU, size:{bigBuffer.Length} but max message size is {MAX_PACKET_SIZE - 3}");
            Assert.That(exception, Has.Message.EqualTo(expected.Message));
        }

        /// <summary>
        /// Checks that batched message were sent as 1 packet
        /// </summary>
        /// <param name="messageLengths"></param>
        private void AssertSentPacket(IEnumerable<int> messageLengths)
        {
            var totalLength = 1 + (2 * messageLengths.Count()) + messageLengths.Sum();

            var mockSocket = Socket.AsMock();

            mockSocket.AssertSendCall(1, null, totalLength);

            // check packet was correct
            CheckMessage(mockSocket.GetLastSendArray(), messageLengths);

            // clear calls after, so we are ready to process next message
            mockSocket.ClearSendAndReceivedCalls();
        }

        private void CheckMessage(byte[] packet, IEnumerable<int> messageLengths)
        {
            if (packet[0] != (byte)PacketType.Reliable)
                Assert.Fail($"First byte was not Reliable, it was {packet[0]} instead");

            var offset = 1;
            foreach (var length in messageLengths)
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
                Socket.AsMock().AssertSendDidNotReceive();
            }

            // should be 97 in buffer now => 1+(length+2)*3
            _connection.SendReliable(_buffer, 0, overBatch);
            AssertSentPacket(lessThanBatchLengths);
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
                    AssertSentPacket(currentBatch);

                    currentBatch.Clear();
                    // new batch
                    total = 1 + 2 + length;
                }
                else
                {
                    _connection.SendReliable(_buffer, 0, length);
                    Socket.AsMock().AssertSendDidNotReceive();
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
                Socket.AsMock().AssertSendDidNotReceive();
            }

            _connection.FlushBatch();
            AssertSentPacket(lessThanBatchLengths);
        }

        [Test]
        public void FlushDoesNotSendEmptyMessage()
        {
            _connection.FlushBatch();
            Socket.AsMock().AssertSendDidNotReceive();
            _connection.FlushBatch();
            Socket.AsMock().AssertSendDidNotReceive();
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
            ((NoReliableConnection)_connection).ReceiveReliablePacket(new Packet(receive.array.AsSpan(0, offset)));
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

            AssertSentPacket(counts);
        }

        [Test]
        public void SendingToNotifyUsesReliable()
        {
            var counts = new List<int>() { 10, 20 };
            _connection.SendNotify(_buffer, 0, counts[0]);
            _connection.SendNotify(_buffer, 0, counts[1]);
            _connection.FlushBatch();

            AssertSentPacket(counts);
        }
        [Test]
        public void SendingToNotifyTokenUsesReliable()
        {
            var token = Substitute.For<INotifyCallBack>();
            var counts = new List<int>() { 10, 20 };
            _connection.SendNotify(_buffer, 0, counts[0], token);
            _connection.SendNotify(_buffer, 0, counts[1], token);
            _connection.FlushBatch();

            AssertSentPacket(counts);
        }

        [Test]
        public void NotifyOnDeliveredInvoke()
        {
            var counts = new List<int>() { 10, 20 };
            var token = _connection.SendNotify(_buffer, 0, counts[0]);
            Assert.That(token, Is.TypeOf<AutoCompleteToken>());
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
