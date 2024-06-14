using System;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using NUnit.Framework;

namespace Mirage.SocketLayer.Tests
{
    [Category("SocketLayer")]
    public class PassthroughConnectionTest : ConnectionTestBase
    {
        private new Connection _connection => (Connection)base._connection;

        protected override Config CreateConfig()
        {
            return new Config
            {
                PassthroughReliableLayer = true,
            };
        }

        [Test]
        public void IsNoReliableConnection()
        {
            Assert.That(_connection, Is.TypeOf<PassthroughConnection>());
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void ThrowsIfTooBig(bool reliable)
        {
            // 3 byte header, so max size is over max
            var bigBuffer = new byte[MAX_PACKET_SIZE - 2];

            var exception = Assert.Throws<ArgumentException>(() =>
            {
                Send(reliable, bigBuffer, bigBuffer.Length);
            });

            var expected = new ArgumentException($"Message is bigger than MTU, size:{bigBuffer.Length} but max message size is {MAX_PACKET_SIZE - 3}");
            Assert.That(exception, Has.Message.EqualTo(expected.Message));
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void MessageAreBatched(bool reliable)
        {
            // max is 100

            var lessThanBatchLengths = new int[]
            {
                20, 40, 30
            };
            var overBatch = 11;

            foreach (var length in lessThanBatchLengths)
            {
                Send(reliable, _buffer, length);
                Socket.DidNotReceive().Send(Arg.Any<IEndPoint>(), Arg.Any<byte[]>(), Arg.Any<int>());
            }

            // should be 97 in buffer now => 1+(length+2)*3
            Send(reliable, _buffer, overBatch);
            AssertSentPacket(reliable ? PacketType.Reliable : PacketType.Unreliable, lessThanBatchLengths);
        }

        [Test]
        [Repeat(100)]
        [TestCase(10)]
        [TestCase(100)]
        public void MessageAreBatched_Repeat(int messageCount)
        {
            var lengths = new int[messageCount];
            for (var i = 0; i < messageCount; i++)
                lengths[i] = rand.Next(10, MAX_PACKET_SIZE - 3);

            var currentBatch_reliable = new List<int>();
            var currentBatch_unreliable = new List<int>();
            var total_reliable = 1;
            var total_unreliable = 1;
            foreach (var length in lengths)
            {
                var reliable = rand.Next(0, 1) == 1;
                if (reliable)
                    SendIntoBatch(length, true, ref total_reliable, currentBatch_reliable);
                else
                    SendIntoBatch(length, false, ref total_unreliable, currentBatch_unreliable);
            }
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void FlushSendsMessageInBatch(bool reliable)
        {
            // max is 100

            var lessThanBatchLengths = new int[]
            {
                20, 40
            };

            foreach (var length in lessThanBatchLengths)
            {
                Send(reliable, _buffer, length);
                Socket.DidNotReceive().Send(Arg.Any<IEndPoint>(), Arg.Any<byte[]>(), Arg.Any<int>());
            }

            _connection.FlushBatch();
            AssertSentPacket(reliable ? PacketType.Reliable : PacketType.Unreliable, lessThanBatchLengths);
        }

        [Test]
        public void FlushSendsMessageInBatch_BothTypes()
        {
            // max is 100

            var lessThanBatchLengths_reliable = new int[]
            {
                20, 40
            };
            var lessThanBatchLengths_unreliable = new int[]
            {
                15, 35, 20
            };

            foreach (var length in lessThanBatchLengths_reliable)
            {
                _connection.SendReliable(_buffer, 0, length);
                Socket.DidNotReceive().Send(Arg.Any<IEndPoint>(), Arg.Any<byte[]>(), Arg.Any<int>());
            }
            foreach (var length in lessThanBatchLengths_unreliable)
            {
                _connection.SendUnreliable(_buffer, 0, length);
                Socket.DidNotReceive().Send(Arg.Any<IEndPoint>(), Arg.Any<byte[]>(), Arg.Any<int>());
            }

            _connection.FlushBatch();

            var totalLength_reliable = 1 + (2 * lessThanBatchLengths_reliable.Count()) + lessThanBatchLengths_reliable.Sum();
            var totalLength_unreliable = 1 + (2 * lessThanBatchLengths_unreliable.Count()) + lessThanBatchLengths_unreliable.Sum();

            // only 2 at any length
            Socket.Received(2).Send(Arg.Any<IEndPoint>(), Arg.Any<byte[]>(), Arg.Any<int>());
            // but also check we received length
            Socket.Received(1).Send(Arg.Any<IEndPoint>(), Arg.Any<byte[]>(), totalLength_reliable);
            Socket.Received(1).Send(Arg.Any<IEndPoint>(), Arg.Any<byte[]>(), totalLength_unreliable);

            // check packet was correct
            CheckMessage(PacketType.Reliable, 0, 2, lessThanBatchLengths_reliable);
            CheckMessage(PacketType.Unreliable, 1, 2, lessThanBatchLengths_unreliable);

            // clear calls after, so we are ready to process next message
            Socket.ClearReceivedCalls();
            _sentArrays.Clear();
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
        [TestCase(true)]
        [TestCase(false)]
        public void UnbatchesMessageOnReceive(bool reliable)
        {
            var receive = _bufferPool.Take();
            receive.array[0] = (byte)(reliable ? PacketType.Reliable : PacketType.Unreliable);
            var offset = 1;
            AddMessage(receive.array, ref offset, 10);
            AddMessage(receive.array, ref offset, 30);
            AddMessage(receive.array, ref offset, 20);

            var segments = new List<ArraySegment<byte>>();
            _peerInstance.dataHandler
                .When(x => x.ReceiveMessage(_connection, Arg.Any<ArraySegment<byte>>()))
                .Do(x => segments.Add(x.ArgAt<ArraySegment<byte>>(1)));
            if (reliable)
                _connection.ReceiveReliablePacket(new Packet(receive, offset));
            else
                _connection.ReceiveUnreliablePacket(new Packet(receive, offset));
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
        public void SendingToUnreliableUsesUnreliable()
        {
            var counts = new List<int>() { 10, 20 };
            _connection.SendUnreliable(_buffer, 0, counts[0]);
            _connection.SendUnreliable(_buffer, 0, counts[1]);
            _connection.FlushBatch();

            AssertSentPacket(PacketType.Unreliable, counts);
        }

        [Test]
        public void SendingToNotifyUsesUnreliable()
        {
            var counts = new List<int>() { 10, 20 };
            _connection.SendNotify(_buffer, 0, counts[0]);
            _connection.SendNotify(_buffer, 0, counts[1]);
            _connection.FlushBatch();

            // only 1 at any length
            Socket.Received(2).Send(Arg.Any<IEndPoint>(), Arg.Any<byte[]>(), Arg.Any<int>());
            // but also check we received length
            Socket.Received(1).Send(Arg.Any<IEndPoint>(), Arg.Any<byte[]>(), AckSystem.NOTIFY_HEADER_SIZE + counts[0]);
            Socket.Received(1).Send(Arg.Any<IEndPoint>(), Arg.Any<byte[]>(), AckSystem.NOTIFY_HEADER_SIZE + counts[1]);

            // check packet was correct
            CheckMessage(PacketType.Notify, 0, 2, counts.Take(1), AckSystem.NOTIFY_HEADER_SIZE - 1);
            CheckMessage(PacketType.Notify, 1, 2, counts.Skip(1).Take(1), AckSystem.NOTIFY_HEADER_SIZE - 1);

            // clear calls after, so we are ready to process next message
            Socket.ClearReceivedCalls();
            _sentArrays.Clear();
        }
        [Test]
        public void SendingToNotifyTokenUsesUnreliable()
        {
            var token = Substitute.For<INotifyCallBack>();
            var counts = new List<int>() { 10, 20 };
            _connection.SendNotify(_buffer, 0, counts[0], token);
            _connection.SendNotify(_buffer, 0, counts[1], token);
            _connection.FlushBatch();

            // only 1 at any length
            Socket.Received(2).Send(Arg.Any<IEndPoint>(), Arg.Any<byte[]>(), Arg.Any<int>());
            // but also check we received length
            Socket.Received(1).Send(Arg.Any<IEndPoint>(), Arg.Any<byte[]>(), AckSystem.NOTIFY_HEADER_SIZE + counts[0]);
            Socket.Received(1).Send(Arg.Any<IEndPoint>(), Arg.Any<byte[]>(), AckSystem.NOTIFY_HEADER_SIZE + counts[1]);

            // check packet was correct
            CheckMessage(PacketType.Notify, 0, 2, counts.Take(1), AckSystem.NOTIFY_HEADER_SIZE - 1);
            CheckMessage(PacketType.Notify, 1, 2, counts.Skip(1).Take(1), AckSystem.NOTIFY_HEADER_SIZE - 1);

            // clear calls after, so we are ready to process next message
            Socket.ClearReceivedCalls();
            _sentArrays.Clear();
        }

        [Test]
        [Ignore("Not implemented")]
        public void NotifyOnDeliveredInvokeAfterReceivingReply()
        {
            var counts = new List<int>() { 10, 20 };
            var token = _connection.SendNotify(_buffer, 0, counts[0]);

            var action = Substitute.For<Action>();
            token.Delivered += action;
            action.DidNotReceive().Invoke();

            // todo receive message here, and then check if Delivered is infact called
        }

        [Test]
        [Ignore("Not implemented")]
        public void NotifyTokenOnDeliveredInvokeAfterReceivingReply()
        {
            var token = Substitute.For<INotifyCallBack>();
            var counts = new List<int>() { 10, 20 };
            _connection.SendNotify(_buffer, 0, counts[0], token);
            token.DidNotReceive().OnDelivered();

            // todo receive message here, and then check if Delivered is infact called
        }
    }
}
