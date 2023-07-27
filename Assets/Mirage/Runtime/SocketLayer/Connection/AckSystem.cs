using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Mirage.SocketLayer
{
    public class AckSystem
    {
        private const int MASK_SIZE = sizeof(ulong) * 8;

        public const int SEQUENCE_HEADER = sizeof(byte) + sizeof(ushort) + sizeof(ushort) + sizeof(ulong);

        /// <summary>PacketType, sequence, ack sequence, mask</summary>
        public const int NOTIFY_HEADER_SIZE = SEQUENCE_HEADER;
        /// <summary>PacketType, sequence, ack sequence, mask, order</summary>
        public const int RELIABLE_HEADER_SIZE = SEQUENCE_HEADER + sizeof(ushort);


        /// <summary>PacketType, ack sequence, mask</summary>
        public const int ACK_HEADER_SIZE = sizeof(byte) + sizeof(ushort) + sizeof(ulong);

        public const int RELIABLE_MESSAGE_LENGTH_SIZE = sizeof(ushort);
        public const int FRAGMENT_INDEX_SIZE = sizeof(byte);

        /// <summary>Smallest size a header for reliable packet, <see cref="RELIABLE_HEADER_SIZE"/> + 2 bytes per message</summary>
        public const int MIN_RELIABLE_HEADER_SIZE = RELIABLE_HEADER_SIZE + RELIABLE_MESSAGE_LENGTH_SIZE;

        /// <summary>Smallest size a header for reliable packet, <see cref="RELIABLE_HEADER_SIZE"/> + 1 byte for fragment index</summary>
        public const int MIN_RELIABLE_FRAGMENT_HEADER_SIZE = RELIABLE_HEADER_SIZE + FRAGMENT_INDEX_SIZE;
        private readonly RingBuffer<AckablePacket> _sentAckablePackets;
        private readonly Sequencer _reliableOrder;
        private readonly RingBuffer<ReliableReceived> _reliableReceive;

        // temp list for resending when processing sentqueue
        private readonly HashSet<ReliablePacket> _toResend = new HashSet<ReliablePacket>();
        private readonly IRawConnection _connection;
        private readonly ITime _time;
        private readonly Pool<ByteBuffer> _bufferPool;
        private readonly Pool<ReliablePacket> _reliablePool;
        private readonly Metrics _metrics;
        private readonly int _maxPacketsInSendBufferPerConnection;
        private readonly int _maxPacketSize;
        private readonly float _ackTimeout;

        /// <summary>how many empty acks to send</summary>
        private readonly int _emptyAckLimit;
        private readonly int _receivesBeforeEmpty;
        private readonly bool _allowFragmented;
        private readonly int _maxFragments;
        private readonly int _maxFragmentsMessageSize;

        public readonly int SizePerFragment;

        /// <summary>
        /// most recent sequence received
        /// <para>will be sent with next message</para>
        /// </summary>
        private ushort _latestAckSequence;

        /// <summary>
        /// mask of recent sequences received
        /// <para>will be sent with next message</para>
        /// </summary>
        private ulong _ackMask;
        private float _lastSentTime;
        private ushort _lastSentAck;
        private int _emptyAckCount = 0;
        private ReliablePacket _nextBatch;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="ackTimeout">how long after last send before sending empty ack</param>
        /// <param name="time"></param>
        public AckSystem(IRawConnection connection, Config config, int maxPacketSize, ITime time, Pool<ByteBuffer> bufferPool, Metrics metrics = null)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            _connection = connection;
            _time = time;
            _bufferPool = bufferPool;
            _reliablePool = new Pool<ReliablePacket>(ReliablePacket.CreateNew, 0, config.MaxReliablePacketsInSendBufferPerConnection);
            _metrics = metrics;

            _ackTimeout = config.TimeBeforeEmptyAck;
            _emptyAckLimit = config.EmptyAckLimit;
            _receivesBeforeEmpty = config.ReceivesBeforeEmptyAck;
            _maxPacketSize = maxPacketSize;
            _maxPacketsInSendBufferPerConnection = config.MaxReliablePacketsInSendBufferPerConnection;

            _maxFragments = config.MaxReliableFragments;
            _allowFragmented = _maxFragments >= 0;
            SizePerFragment = maxPacketSize - MIN_RELIABLE_FRAGMENT_HEADER_SIZE;
            _maxFragmentsMessageSize = _maxFragments * SizePerFragment;

            var size = config.SequenceSize;
            if (size > 16) throw new ArgumentOutOfRangeException("SequenceSize", size, "SequenceSize has a max value of 16");
            _sentAckablePackets = new RingBuffer<AckablePacket>(size);
            _reliableOrder = new Sequencer(size);
            _reliableReceive = new RingBuffer<ReliableReceived>(size);

            // set lastest to value before 0 so that first packet will be received
            // max will be 1 less than 0
            _latestAckSequence = (ushort)_sentAckablePackets.Sequencer.MoveInBounds(ulong.MaxValue);

            OnSend();
        }

        /// <summary>
        /// Gets next Reliable packet in order, packet consists for multiple messsages
        /// <para>[length, message, length, message, ...]</para>
        /// </summary>
        /// <param name="packet"></param>
        /// <returns>true if next packet is available</returns>
        public bool NextReliablePacket(out ReliableReceived packet)
        {
            if (!_reliableReceive.TryPeak(out packet))
                return false;


            // normal packet (with batched messages) OR full fragmented message
            if (!packet.IsFragment || CheckFullFragmentedMessage(packet, _reliableReceive.Read))
            {
                _reliableReceive.RemoveNext();
                return true;
            }

            return false;
        }

        private bool CheckFullFragmentedMessage(ReliableReceived packet, uint readIndex)
        {
            // fragment will always be first byte of message
            uint fragmentIndex = packet.Buffer.array[0];

            // if fragment Index is 3 we expect 4 packets total (3 more)
            // so we check 0,1,2 packets in
            var fullMessage = true;
            for (uint i = 0; i < fragmentIndex; i++)
            {
                // check if other packets after current exist
                if (!_reliableReceive.Exists(readIndex + i + 1))
                {
                    fullMessage = false;
                    break;
                }
            }

            return fullMessage;
        }

        public ReliableReceived GetNextFragment()
        {
            return _reliableReceive.Dequeue();
        }

        public void Update()
        {
            if (_nextBatch != null)
            {
                SendReliablePacket(_nextBatch);
                _nextBatch = null;
            }


            // todo send ack if not recently been sent
            // ack only packet sent if no other sent within last frame
            if (ShouldSendEmptyAck() && TimeToSendAck())
            {
                // send ack
                SendAck();
            }
        }

        /// <summary>
        /// resets empty ack count, this should be called after LatestAckSequence increases
        /// </summary>
        private void ResetEmptyAckCount()
        {
            _emptyAckCount = 0;
        }

        private void CheckSendEmptyAck()
        {
            var distance = _sentAckablePackets.Sequencer.Distance(_latestAckSequence, _lastSentAck);
            if (distance > _receivesBeforeEmpty)
            {
                SendAck();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TimeToSendAck()
        {
            var shouldSend = _lastSentTime + _ackTimeout < _time.Now;
            return shouldSend;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool ShouldSendEmptyAck()
        {
            return _emptyAckCount < _emptyAckLimit;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Send(byte[] final, int length)
        {
            _connection.SendRaw(final, length);
            OnSend();
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnSend()
        {
            _emptyAckCount++;
            _lastSentAck = _latestAckSequence;
            _lastSentTime = _time.Now;
        }

        private void SendAck()
        {
            using (var final = _bufferPool.Take())
            {
                var offset = 0;

                ByteUtils.WriteByte(final.array, ref offset, (byte)PacketType.Ack);

                ByteUtils.WriteUShort(final.array, ref offset, _latestAckSequence);
                ByteUtils.WriteULong(final.array, ref offset, _ackMask);

                _connection.SendRaw(final.array, offset);
                Send(final.array, offset);
            }
        }

        /// <summary>
        /// Use <see cref="SendNotify(byte[], int, int, INotifyCallBack)"/> for non-alloc version
        /// </summary>
        public INotifyToken SendNotify(byte[] inPacket, int inOffset, int inLength)
        {
            var token = new NotifyToken();
            SendNotify(inPacket, inOffset, inLength, token);
            return token;
        }

        public void SendNotify(byte[] inPacket, int inOffset, int inLength, INotifyCallBack callBacks)
        {
            if (inLength + NOTIFY_HEADER_SIZE > _maxPacketSize)
            {
                throw new ArgumentException($"Message is bigger than MTU, size:{inLength} but max Notify message size is {_maxPacketSize - NOTIFY_HEADER_SIZE}");
            }
            if (_sentAckablePackets.IsFull)
            {
                throw new InvalidOperationException("Sent queue is full");
            }

            var sequence = (ushort)_sentAckablePackets.Enqueue(new AckablePacket(callBacks));

            using (var buffer = _bufferPool.Take())
            {
                var outPacket = buffer.array;
                Buffer.BlockCopy(inPacket, inOffset, outPacket, NOTIFY_HEADER_SIZE, inLength);

                var outOffset = 0;

                ByteUtils.WriteByte(outPacket, ref outOffset, (byte)PacketType.Notify);

                ByteUtils.WriteUShort(outPacket, ref outOffset, sequence);
                ByteUtils.WriteUShort(outPacket, ref outOffset, _latestAckSequence);
                ByteUtils.WriteULong(outPacket, ref outOffset, _ackMask);

                Send(outPacket, outOffset + inLength);
            }
        }



        public void SendReliable(byte[] message, int offset, int length)
        {
            if (_sentAckablePackets.IsFull)
            {
                throw new InvalidOperationException($"Sent queue is full for {_connection}");
            }

            if (length + MIN_RELIABLE_HEADER_SIZE > _maxPacketSize)
            {
                if (!_allowFragmented)
                    throw new ArgumentException($"Message is bigger than MTU and fragmentation is disabled, max Reliable message size is {_maxPacketSize - MIN_RELIABLE_HEADER_SIZE}", nameof(length));

                // if there is existing batch, send it first
                // we need to do this so that fragmented message arrive in order
                // if we dont, a message sent after maybe be added to batch and then have earlier order than fragmented message
                if (_nextBatch != null)
                {
                    SendReliablePacket(_nextBatch);
                    _nextBatch = null;
                }

                SendFragmented(message, offset, length);
                return;
            }


            if (_nextBatch == null)
            {
                _nextBatch = CreateReliableBuffer(PacketType.Reliable);
            }

            var msgLength = length + RELIABLE_MESSAGE_LENGTH_SIZE;
            var batchLength = _nextBatch.Length;
            if (batchLength + msgLength > _maxPacketSize)
            {
                // if full, send and create new
                SendReliablePacket(_nextBatch);

                _nextBatch = CreateReliableBuffer(PacketType.Reliable);
            }

            AddToBatch(_nextBatch, message, offset, length);
        }

        /// <summary>
        /// Splits messsage into multiple packets
        /// <para>Note: this might just send 1 packet if length is equal to size.
        /// This might happen because fragmented header is 1 less that batched header</para>
        /// </summary>
        /// <param name="message"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        private void SendFragmented(byte[] message, int offset, int length)
        {
            if (length > _maxFragmentsMessageSize)
            {
                throw new ArgumentException($"Message is bigger than MTU for fragmentation, max Reliable fragmented size is {_maxFragmentsMessageSize}", nameof(length));
            }

            var fragments = Mathf.CeilToInt(length / (float)SizePerFragment);

            var remaining = length;
            for (var i = 0; i < fragments; i++)
            {
                var fragmentIndex = fragments - i - 1;

                var packet = CreateReliableBuffer(PacketType.ReliableFragment);
                var array = packet.Buffer.array;
                var packetOffset = packet.Length;

                ByteUtils.WriteByte(array, ref packetOffset, (byte)fragmentIndex);
                var nextLength = Math.Min(remaining, SizePerFragment);
                Buffer.BlockCopy(message, offset + (SizePerFragment * i), array, packetOffset, nextLength);
                packetOffset += nextLength;
                remaining -= nextLength;

                packet.Length = packetOffset;
                SendReliablePacket(packet);
            }
        }

        private ReliablePacket CreateReliableBuffer(PacketType packetType)
        {
            var order = (ushort)_reliableOrder.Next();

            var packet = _reliablePool.Take();
            var buffer = _bufferPool.Take();

            var offset = 0;
            ByteUtils.WriteByte(buffer.array, ref offset, (byte)packetType);

            offset = SEQUENCE_HEADER;
            ByteUtils.WriteUShort(buffer.array, ref offset, order);

            packet.Setup(order, buffer, RELIABLE_HEADER_SIZE);
            return packet;
        }

        private static void AddToBatch(ReliablePacket packet, byte[] message, int offset, int length)
        {
            var array = packet.Buffer.array;
            var packetOffset = packet.Length;

            ByteUtils.WriteUShort(array, ref packetOffset, (ushort)length);
            Buffer.BlockCopy(message, offset, array, packetOffset, length);
            packetOffset += length;

            packet.Length = packetOffset;
        }

        private void SendReliablePacket(ReliablePacket reliable)
        {
            ThrowIfBufferLimitReached();

            var sequence = (ushort)_sentAckablePackets.Enqueue(new AckablePacket(reliable));

            var final = reliable.Buffer.array;

            var offset = 1;

            reliable.OnSend(sequence);
            ByteUtils.WriteUShort(final, ref offset, sequence);
            ByteUtils.WriteUShort(final, ref offset, _latestAckSequence);
            ByteUtils.WriteULong(final, ref offset, _ackMask);

            Send(final, reliable.Length);
        }

        private void ThrowIfBufferLimitReached()
        {
            // greater or equal, because we are adding 1 adder this check
            if (_sentAckablePackets.Count >= _maxPacketsInSendBufferPerConnection)
            {
                throw new InvalidOperationException($"Max packets in send buffer reached for {_connection}");
            }
        }


        /// <summary>
        /// Receives incoming Notify packet
        /// <para>Ignores duplicate or late packets</para>
        /// </summary>
        /// <param name="packet"></param>
        /// <returns>default or new packet to handle</returns>
        public ArraySegment<byte> ReceiveNotify(byte[] packet, int length)
        {
            // start at 1 to skip packet type
            var offset = 1;

            var sequence = ByteUtils.ReadUShort(packet, ref offset);
            var ackSequence = ByteUtils.ReadUShort(packet, ref offset);
            var ackMask = ByteUtils.ReadULong(packet, ref offset);

            var distance = ProcessIncomingHeader(sequence, ackSequence, ackMask);

            // duplicate or arrived late
            if (distance <= 0) { return default; }

            var segment = new ArraySegment<byte>(packet, NOTIFY_HEADER_SIZE, length - NOTIFY_HEADER_SIZE);
            return segment;
        }


        /// <summary>
        /// Checks if fragment index is less than max fragment size
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public bool InvalidFragment(byte[] array)
        {
            var offset = RELIABLE_HEADER_SIZE;
            var fragmentIndex = ByteUtils.ReadByte(array, ref offset);

            // invalid if equal to (because it should be 0 indexed)
            return fragmentIndex >= _maxFragments;
        }

        /// <summary>
        /// </summary>
        /// <param name="packet"></param>
        /// <returns>true if there are ordered message to read</returns>
        public void ReceiveReliable(byte[] packet, int length, bool isFragment)
        {
            // start at 1 to skip packet type
            var offset = 1;

            var sequence = ByteUtils.ReadUShort(packet, ref offset);
            var ackSequence = ByteUtils.ReadUShort(packet, ref offset);
            var ackMask = ByteUtils.ReadULong(packet, ref offset);
            var reliableSequence = ByteUtils.ReadUShort(packet, ref offset);

            _ = ProcessIncomingHeader(sequence, ackSequence, ackMask);

            // checks acks, late message are allowed for reliable
            // but only insert lastest if later than read Index

            var reliableDistance = _reliableReceive.DistanceToRead(reliableSequence);

            if (reliableDistance < 0)
            {
                // old packet
                return;
            }


            if (PacketExists(reliableSequence))
            {
                // packet already received 
                return;
            }

            // new packet
            var savedPacket = _bufferPool.Take();
            var bufferLength = length - RELIABLE_HEADER_SIZE;
            Buffer.BlockCopy(packet, RELIABLE_HEADER_SIZE, savedPacket.array, 0, bufferLength);
            _reliableReceive.InsertAt(reliableSequence, new ReliableReceived(savedPacket, bufferLength, isFragment));
        }

        private bool PacketExists(ushort reliableSequence)
        {
            var existing = _reliableReceive[reliableSequence];
            return existing.Buffer != null;
        }

        public void ReceiveAck(byte[] packet)
        {
            // start at 1 to skip packet type
            var offset = 1;

            var ackSequence = ByteUtils.ReadUShort(packet, ref offset);
            var ackMask = ByteUtils.ReadULong(packet, ref offset);

            CheckSentQueue(ackSequence, ackMask);
        }

        /// <returns>distance</returns>
        private int ProcessIncomingHeader(ushort sequence, ushort ackSequence, ulong ackMask)
        {
            var distance = (int)_sentAckablePackets.Sequencer.Distance(sequence, _latestAckSequence);
            SetAckValues(sequence, distance);
            CheckSentQueue(ackSequence, ackMask);
            return distance;
        }

        private void SetAckValues(ushort sequence, long distance)
        {
            if (distance > 0)
            {
                // distance is too large to be shifted
                if (distance >= MASK_SIZE)
                {
                    // this means 63 packets have gone missingg
                    // this should never happen, but if it does then just set mask to 1
                    _ackMask = 1;
                }
                else
                {
                    // shift mask by distance, then add 1
                    // eg distance = 2
                    // this will mean mask will be ..01
                    // which means that 1 packet was missed
                    _ackMask = (_ackMask << (int)distance) | 1;
                }
                _latestAckSequence = sequence;
            }
            else
            {
                var negativeDistance = -(int)distance;

                // distance is too large to be shifted
                if (negativeDistance >= MASK_SIZE)
                    return;

                var newAck = 1ul << negativeDistance;
                _ackMask |= newAck;
            }

            // after receiving reset empty count and check if we should send ack right away
            ResetEmptyAckCount();
            CheckSendEmptyAck();
        }

        private void CheckSentQueue(ushort sequence, ulong mask)
        {
            // old sequence, nothing in buffer to ack/lost
            if (_sentAckablePackets.DistanceToRead(sequence) < 0) { return; }

            AckMessagesInSentQueue(sequence, mask);
            _sentAckablePackets.MoveReadToNextNonEmpty();
            ResendMessages();
        }
        private void AckMessagesInSentQueue(ushort sequence, ulong mask)
        {
            var start = _sentAckablePackets.Read;
            var end = _sentAckablePackets.Write;
            var sequencer = _sentAckablePackets.Sequencer;

            var count = sequencer.Distance(end, start);

            for (uint i = 0; i < count; i++)
            {
                var ackableSequence = (uint)sequencer.MoveInBounds(start + i);
                var ackable = _sentAckablePackets[ackableSequence];

                if (ackable.IsNotValid())
                    continue;

                CheckAckablePacket(sequence, mask, ackable, ackableSequence);
            }
        }

        private void CheckAckablePacket(ushort sequence, ulong mask, AckablePacket ackable, uint ackableSequence)
        {
            var distance = (int)_sentAckablePackets.Sequencer.Distance(sequence, ackableSequence);

            // negative distance means next is sent after last ack, so nothing to ack yet
            // no chance for it to be acked yet, so do nothing
            if (distance < 0)
                return;


            var lost = OutsideOfMask(distance) || NotInMask(distance, mask);

            if (ackable.IsNotify)
            {
                ackable.Token.Notify(!lost);
                _sentAckablePackets.RemoveAt(ackableSequence);
            }
            else
            {
                var reliablePacket = ackable.ReliablePacket;
                if (lost)
                {
                    ReliableLost(sequence, reliablePacket);
                }
                else
                {
                    ReliableAcked(reliablePacket);
                }
            }
        }

        private void ReliableAcked(ReliablePacket reliablePacket)
        {
            foreach (var seq in reliablePacket.Sequences)
            {
                _sentAckablePackets.RemoveAt(seq);
            }

            // remove from toResend incase it was added in previous loop
            _toResend.Remove(reliablePacket);

            reliablePacket.OnAck();
        }

        private void ReliableLost(ushort sequence, ReliablePacket reliablePacket)
        {
            // we dont need to resend if it has not been possible to have been acked yet

            // eg seq=99, last = 101 => dist = 99-101=-2 => sent after seq => dont resend
            // => if positive, then resend
            if (_sentAckablePackets.Sequencer.Distance(sequence, reliablePacket.LastSequence) > 0)
            {
                _toResend.Add(reliablePacket);
            }
        }

        private void ResendMessages()
        {
            foreach (var reliable in _toResend)
            {
                _metrics?.OnResend(reliable.Length);
                SendReliablePacket(reliable);
            }
            _toResend.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool OutsideOfMask(int distance)
        {
            // if distance is 64 or greater
            // important: this check is to stop the bitshift from breaking!!
            // bit shifting only uses first 6 bits of RHS (64->0 65->1) so higher number wont shift correct and ack wrong packet
            return distance >= MASK_SIZE;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool NotInMask(int distance, ulong receivedMask)
        {
            var ackBit = 1ul << distance;
            return (receivedMask & ackBit) == 0u;
        }

        private struct AckablePacket : IEquatable<AckablePacket>
        {
            public INotifyCallBack Token;
            public ReliablePacket ReliablePacket;

            public bool IsNotify => Token != null;
            public bool IsReliable => ReliablePacket != null;

            public AckablePacket(INotifyCallBack token)
            {
                Token = token;
                ReliablePacket = null;
            }

            public AckablePacket(ReliablePacket reliablePacket)
            {
                ReliablePacket = reliablePacket;
                Token = null;
            }

            public bool Equals(AckablePacket other)
            {
                return Token == other.Token &&
                    ReliablePacket == other.ReliablePacket;
            }

            /// <summary>
            /// returns true if this is default value of struct
            /// </summary>
            /// <returns></returns>
            public bool IsNotValid()
            {
                return Token == null && ReliablePacket == null;
            }
        }

        private class ReliablePacket
        {
            public ushort LastSequence;
            public int Length;

            public ByteBuffer Buffer;
            public ushort Order;

            public readonly List<ushort> Sequences = new List<ushort>(4);
            private readonly Pool<ReliablePacket> _pool;

            public void OnSend(ushort sequence)
            {
                Sequences.Add(sequence);
                LastSequence = sequence;
            }

            public void OnAck()
            {
                Buffer.Release();
                _pool.Put(this);
            }

            public void Setup(ushort order, ByteBuffer buffer, int length)
            {
                // reset old data
                LastSequence = 0;
                Sequences.Clear();

                Order = order;
                Buffer = buffer;
                Length = length;
            }

            private ReliablePacket(Pool<ReliablePacket> pool)
            {
                _pool = pool;
            }

            public override int GetHashCode()
            {
                return Order;
            }

            public override bool Equals(object obj)
            {
                if (obj is ReliablePacket other)
                {
                    // use order as quick check, but use list to check if they are actually equal
                    return Order == other.Order && Sequences == other.Sequences;
                }
                return false;
            }

            public static ReliablePacket CreateNew(Pool<ReliablePacket> pool)
            {
                return new ReliablePacket(pool);
            }
        }
        public struct ReliableReceived : IEquatable<ReliableReceived>
        {
            public readonly ByteBuffer Buffer;
            public readonly int Length;
            public readonly bool IsFragment;

            public int FragmentIndex
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => Buffer.array[0];
            }

            public ReliableReceived(ByteBuffer buffer, int length, bool isFragment)
            {
                Buffer = buffer;
                Length = length;
                IsFragment = isFragment;
            }

            public bool Equals(ReliableReceived other)
            {
                return Buffer == other.Buffer;
            }
        }
    }
}
