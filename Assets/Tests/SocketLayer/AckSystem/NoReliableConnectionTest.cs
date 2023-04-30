using System;
using System.Collections.Generic;
using System.Linq;
using Mirage.SocketLayer.Tests.PeerTests;
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
        private readonly Random rand = new Random();
        private byte[] _sentArray;

        private ISocket Socket => _peerInstance.socket;

        [SetUp]
        public void Setup()
        {
            _config = new Config
            {
                DisableReliableLayer = true,
            };
            _peerInstance = new PeerInstance(_config, maxPacketSize: MAX_PACKET_SIZE);
            _connection = _peerInstance.peer.Connect(Substitute.For<IEndPoint>());

            _buffer = new byte[MAX_PACKET_SIZE - 1];
            for (var i = 0; i < _buffer.Length; i++)
            {
                _buffer[i] = (byte)i;
            }

            // clear calls, Connect will have sent one
            Socket.ClearReceivedCalls();
            Socket.When(x => x.Send(Arg.Any<IEndPoint>(), Arg.Any<byte[]>(), Arg.Any<int>()))
                .Do(x =>
                {
                    var arg = (byte[])x.Args()[1];
                    // create copy
                    _sentArray = arg.ToArray();
                });
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

        private void AssertSentPacket(int totalLength, IEnumerable<int> messageLengths)
        {
            // only 1 at any length
            Socket.Received(1).Send(Arg.Any<IEndPoint>(), Arg.Any<byte[]>(), Arg.Any<int>());
            // but also check we received length
            Socket.Received(1).Send(Arg.Any<IEndPoint>(), Arg.Any<byte[]>(), totalLength);

            // check packet was correct
            CheckMessage(_sentArray, messageLengths);

            // clear calls after, so we are ready to process next message
            Socket.ClearReceivedCalls();
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

            var total = 1;
            foreach (var length in lessThanBatchLengths)
            {
                // will write length+2
                total += 2 + length;
                _connection.SendReliable(_buffer, 0, length);
                Socket.DidNotReceive().Send(Arg.Any<IEndPoint>(), Arg.Any<byte[]>(), Arg.Any<int>());
            }

            // should be 97 in buffer now => 1+(length+2)*3
            _connection.SendReliable(_buffer, 0, overBatch);
            AssertSentPacket(total, lessThanBatchLengths);
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
                    AssertSentPacket(total, currentBatch);

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

            var total = 1;
            foreach (var length in lessThanBatchLengths)
            {
                // will write length+2
                total += 2 + length;
                _connection.SendReliable(_buffer, 0, length);
                Socket.DidNotReceive().Send(Arg.Any<IEndPoint>(), Arg.Any<byte[]>(), Arg.Any<int>());
            }

            _connection.FlushBatch();
            AssertSentPacket(total, lessThanBatchLengths);
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
        public void SendingToOtherChannelsUsesReliable()
        {
            throw new System.NotImplementedException();
        }

        [Test]
        public void UnbatchesMessageOnReceive()
        {
            throw new System.NotImplementedException();
        }
    }
}
