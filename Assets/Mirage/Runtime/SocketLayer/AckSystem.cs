using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Mirage.SocketLayer
{
    internal class AckSystem
    {
        const int MASK_SIZE = sizeof(ulong) * 8;

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

        readonly RingBuffer<AckablePacket> sentAckablePackets;
        readonly Sequencer reliableOrder;
        readonly RingBuffer<ReliableReceived> reliableReceive;

        // temp list for resending when processing sentqueue
        readonly HashSet<ReliablePacket> toResend = new HashSet<ReliablePacket>();

        readonly IRawConnection connection;
        readonly ITime time;
        readonly BufferPool bufferPool;
        readonly Metrics metrics;

        //todo implement this
        readonly int maxPacketsInSendBufferPerConnection;
        readonly int maxPacketSize;
        readonly float ackTimeout;
        /// <summary>how many empty acks to send</summary>
        readonly int emptyAckLimit;
        readonly int receivesBeforeEmpty;

        /// <summary>
        /// most recent sequence received
        /// <para>will be sent with next message</para>
        /// </summary>
        ushort LatestAckSequence;
        /// <summary>
        /// mask of recent sequences received
        /// <para>will be sent with next message</para>
        /// </summary>
        ulong AckMask;

        float lastSentTime;
        ushort lastSentAck;
        int emptyAckCount = 0;
        ReliablePacket nextBatch;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="ackTimeout">how long after last send before sending empty ack</param>
        /// <param name="time"></param>
        public AckSystem(IRawConnection connection, Config config, ITime time, BufferPool bufferPool, Metrics metrics = null)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            this.connection = connection;
            this.time = time;
            this.bufferPool = bufferPool;
            this.metrics = metrics;

            ackTimeout = config.TimeBeforeEmptyAck;
            emptyAckLimit = config.EmptyAckLimit;
            receivesBeforeEmpty = config.ReceivesBeforeEmptyAck;
            maxPacketSize = config.MaxPacketSize;
            maxPacketsInSendBufferPerConnection = config.MaxReliablePacketsInSendBufferPerConnection;

            int size = config.SequenceSize;
            sentAckablePackets = new RingBuffer<AckablePacket>(size);
            reliableOrder = new Sequencer(size);
            reliableReceive = new RingBuffer<ReliableReceived>(size);

            OnSend();
        }

        /// <summary>
        /// Gets next Reliable packet in order, packet consists for multiple messsages
        /// <para>[length, message, length, message, ...]</para>
        /// </summary>
        /// <param name="packet"></param>
        /// <returns>true if next packet is avaliable</returns>
        public bool NextReliablePacket(out ReliableReceived packet)
        {
            return reliableReceive.TryDequeue(out packet);
        }

        public void Update()
        {
            if (nextBatch != null)
            {
                SendReliablePacket(nextBatch);
                nextBatch = null;
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
        void ResetEmptyAckCount()
        {
            emptyAckCount = 0;
        }
        void CheckSendEmptyAck()
        {
            long distance = sentAckablePackets.Sequencer.Distance(LatestAckSequence, lastSentAck);
            if (distance > receivesBeforeEmpty)
            {
                SendAck();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool TimeToSendAck()
        {
            bool shouldSend = lastSentTime + ackTimeout < time.Now;
            return shouldSend;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool ShouldSendEmptyAck()
        {
            return emptyAckCount < emptyAckLimit;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Send(byte[] final, int length)
        {
            connection.SendRaw(final, length);
            OnSend();
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void OnSend()
        {
            emptyAckCount++;
            lastSentAck = LatestAckSequence;
            lastSentTime = time.Now;
        }

        private void SendAck()
        {
            using (ByteBuffer final = bufferPool.Take())
            {
                int offset = 0;

                ByteUtils.WriteByte(final.array, ref offset, (byte)PacketType.Ack);

                ByteUtils.WriteUShort(final.array, ref offset, LatestAckSequence);
                ByteUtils.WriteULong(final.array, ref offset, AckMask);

                connection.SendRaw(final.array, offset);
                Send(final.array, offset);
            }
        }

        public INotifyToken SendNotify(byte[] inPacket, int inOffset, int inLength)
        {
            // todo batch Notify?
            if (inLength + NOTIFY_HEADER_SIZE > maxPacketSize)
            {
                throw new IndexOutOfRangeException($"Message is bigger than MTU, max Notify message size is {maxPacketSize - NOTIFY_HEADER_SIZE}");
            }
            if (sentAckablePackets.IsFull)
            {
                throw new InvalidOperationException("Sent queue is full");
            }

            // todo use pool to stop allocations
            var token = new NotifyToken();
            ushort sequence = (ushort)sentAckablePackets.Enqueue(new AckablePacket(token));

            using (ByteBuffer buffer = bufferPool.Take())
            {
                byte[] outPacket = buffer.array;
                Buffer.BlockCopy(inPacket, inOffset, outPacket, NOTIFY_HEADER_SIZE, inLength);

                int outOffset = 0;

                ByteUtils.WriteByte(outPacket, ref outOffset, (byte)PacketType.Notify);

                ByteUtils.WriteUShort(outPacket, ref outOffset, sequence);
                ByteUtils.WriteUShort(outPacket, ref outOffset, LatestAckSequence);
                ByteUtils.WriteULong(outPacket, ref outOffset, AckMask);

                Send(outPacket, outOffset + inLength);
            }

            return token;
        }


        public void SendReliable(byte[] message, int offset, int length)
        {
            if (length + RELIABLE_HEADER_SIZE + 2 > maxPacketSize)
            {
                throw new IndexOutOfRangeException($"Message is bigger than MTU, max Reliable message size is {maxPacketSize - RELIABLE_HEADER_SIZE - 2}");
            }
            if (sentAckablePackets.IsFull)
            {
                throw new InvalidOperationException("Sent queue is full");
            }

            if (nextBatch == null)
            {
                ushort order = (ushort)reliableOrder.Next();
                nextBatch = CreateReliableBuffer(order);
            }

            int msgLength = length + 2;
            int batchLength = nextBatch.length;
            if (batchLength + msgLength > maxPacketSize)
            {
                // if full, send and create new
                SendReliablePacket(nextBatch);

                ushort order = (ushort)reliableOrder.Next();
                nextBatch = CreateReliableBuffer(order);
            }

            AddToBatch(nextBatch, message, offset, length);
        }

        private ReliablePacket CreateReliableBuffer(ushort order)
        {

            ByteBuffer final = bufferPool.Take();

            int offset = 0;
            ByteUtils.WriteByte(final.array, ref offset, (byte)PacketType.Reliable);

            offset = 13;
            ByteUtils.WriteUShort(final.array, ref offset, order);

            return new ReliablePacket(final, RELIABLE_HEADER_SIZE, order);
        }
        static void AddToBatch(ReliablePacket packet, byte[] message, int offset, int length)
        {
            byte[] array = packet.buffer.array;
            int packetOffset = packet.length;

            ByteUtils.WriteUShort(array, ref packetOffset, (ushort)length);
            Buffer.BlockCopy(message, offset, array, packetOffset, length);
            packetOffset += length;

            packet.length = packetOffset;
        }

        void SendReliablePacket(ReliablePacket reliable)
        {
            if (reliable.acked) { return; }

            ushort sequence = (ushort)sentAckablePackets.Enqueue(new AckablePacket(reliable));

            byte[] final = reliable.buffer.array;

            int offset = 1;

            reliable.OnSend(sequence);
            ByteUtils.WriteUShort(final, ref offset, sequence);
            ByteUtils.WriteUShort(final, ref offset, LatestAckSequence);
            ByteUtils.WriteULong(final, ref offset, AckMask);

            Send(final, reliable.length);
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
            int offset = 1;

            ushort sequence = ByteUtils.ReadUShort(packet, ref offset);
            ushort ackSequence = ByteUtils.ReadUShort(packet, ref offset);
            ulong ackMask = ByteUtils.ReadULong(packet, ref offset);

            int distance = ProcessIncomingHeader(sequence, ackSequence, ackMask);

            // duplicate or arrived late
            if (distance <= 0) { return default; }

            var segment = new ArraySegment<byte>(packet, NOTIFY_HEADER_SIZE, length - NOTIFY_HEADER_SIZE);
            return segment;
        }


        /// <summary>
        /// </summary>
        /// <param name="packet"></param>
        /// <returns>true if there are ordered message to read</returns>
        public void ReceiveReliable(byte[] packet, int length)
        {
            // start at 1 to skip packet type
            int offset = 1;

            ushort sequence = ByteUtils.ReadUShort(packet, ref offset);
            ushort ackSequence = ByteUtils.ReadUShort(packet, ref offset);
            ulong ackMask = ByteUtils.ReadULong(packet, ref offset);
            ushort reliableSequence = ByteUtils.ReadUShort(packet, ref offset);

            _ = ProcessIncomingHeader(sequence, ackSequence, ackMask);

            // checks acks, late message are allowed for reliable
            // but only update lastest if later than current

            long reliableDistance = reliableReceive.DistanceToRead(reliableSequence);

            if (reliableDistance < 0)
            {
                // old packet
                return;
            }


            ReliableReceived existing = reliableReceive[reliableSequence];
            if (existing.buffer == null)
            {
                // new packet
                ByteBuffer savedPacket = bufferPool.Take();
                int bufferLength = length - RELIABLE_HEADER_SIZE;
                Buffer.BlockCopy(packet, RELIABLE_HEADER_SIZE, savedPacket.array, 0, bufferLength);
                reliableReceive.InsertAt(reliableSequence, new ReliableReceived(savedPacket, bufferLength));
            }
        }


        public void ReceiveAck(byte[] packet)
        {
            // start at 1 to skip packet type
            int offset = 1;

            ushort ackSequence = ByteUtils.ReadUShort(packet, ref offset);
            ulong ackMask = ByteUtils.ReadULong(packet, ref offset);

            CheckSentQueue(ackSequence, ackMask);
        }

        /// <returns>distance</returns>
        int ProcessIncomingHeader(ushort sequence, ushort ackSequence, ulong ackMask)
        {
            int distance = (int)sentAckablePackets.Sequencer.Distance(sequence, LatestAckSequence);
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
                    AckMask = 1;
                }
                else
                {
                    // shift mask by distance, then add 1
                    // eg distance = 2
                    // this will mean mask will be ..01
                    // which means that 1 packet was missed
                    AckMask = (AckMask << (int)distance) | 1;
                }
                LatestAckSequence = sequence;
            }
            else
            {
                int negativeDistnace = -(int)distance;

                // distance is too large to be shifted
                if (negativeDistnace >= MASK_SIZE)
                    return;

                ulong newAck = 1ul << negativeDistnace;
                AckMask |= newAck;
            }

            // after receiving reset empty count and check if we should send ack right away
            ResetEmptyAckCount();
            CheckSendEmptyAck();
        }

        private void CheckSentQueue(ushort sequence, ulong mask)
        {
            // old sequence, nothing in buffer to ack/lost
            if (sentAckablePackets.DistanceToRead(sequence) < 0) { return; }

            ackMessagesInSentQueue(sequence, mask);
            sentAckablePackets.MoveReadToNextNonEmpty();
            resendMessages();
        }
        private void ackMessagesInSentQueue(ushort sequence, ulong mask)
        {
            uint start = sentAckablePackets.Read;
            uint end = sentAckablePackets.Write;
            Sequencer sequencer = sentAckablePackets.Sequencer;

            long count = sequencer.Distance(end, start);

            for (uint i = 0; i < count; i++)
            {
                uint ackableSequence = (uint)sequencer.MoveInBounds(start + i);
                AckablePacket ackable = sentAckablePackets[ackableSequence];

                if (ackable.Equals(default))
                    continue;

                if (alreadyAcked(ackable, ackableSequence))
                    continue;

                CheckAckablePacket(sequence, mask, ackable, ackableSequence);
            }
        }

        private bool alreadyAcked(AckablePacket ackable, uint ackableSequence)
        {
            // if we have already ackeds this, just remove it
            if (ackable.IsReliable && ackable.reliablePacket.acked)
            {
                // this should never happen because packet should be removed when acked is set to true
                // but remove it just incase we get here
                sentAckablePackets.RemoveAt(ackableSequence);
                return true;
            }
            return false;
        }
        private void CheckAckablePacket(ushort sequence, ulong mask, AckablePacket ackable, uint ackableSequence)
        {
            int distance = (int)sentAckablePackets.Sequencer.Distance(sequence, ackableSequence);

            // negative distance means next is sent after last ack, so nothing to ack yet
            // no chance for it to be acked yet, so do nothing
            if (distance < 0)
                return;


            bool lost = OutsideOfMask(distance) || NotInMask(distance, mask);

            if (ackable.IsNotify)
            {
                ackable.token.Notify(!lost);
                sentAckablePackets.RemoveAt(ackableSequence);
            }
            else
            {
                ReliablePacket reliablePacket = ackable.reliablePacket;
                if (lost)
                {
                    reliableLost(sequence, reliablePacket);
                }
                else
                {
                    reliableAcked(reliablePacket);
                }
            }
        }

        private void reliableAcked(ReliablePacket reliablePacket)
        {
            reliablePacket.OnAck();
            foreach (ushort seq in reliablePacket.sequences)
            {
                sentAckablePackets.RemoveAt(seq);
            }
        }

        private void reliableLost(ushort sequence, ReliablePacket reliablePacket)
        {
            // we dont need to resend if it has not been possible to have been acked yet

            // eg seq=99, last = 101 => dist = 99-101=-2 => sent after seq => dont resend
            // => if positive, then resend
            if (sentAckablePackets.Sequencer.Distance(sequence, reliablePacket.lastSequence) > 0)
            {
                toResend.Add(reliablePacket);
            }
        }

        private void resendMessages()
        {
            foreach (ReliablePacket reliable in toResend)
            {
                metrics?.OnResend(reliable.length);
                SendReliablePacket(reliable);
            }
            toResend.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool OutsideOfMask(int distance)
        {
            // if distance is 64 or greater
            // important: this check is to stop the bitshift from breaking!!
            // bit shifting only uses first 6 bits of RHS (64->0 65->1) so higher number wont shift correct and ack wrong packet
            return distance >= MASK_SIZE;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool NotInMask(int distance, ulong receivedMask)
        {
            ulong ackBit = 1ul << distance;
            return (receivedMask & ackBit) == 0u;
        }


        struct AckablePacket : IEquatable<AckablePacket>
        {
            public NotifyToken token;
            public ReliablePacket reliablePacket;

            public bool IsNotify => token != null;
            public bool IsReliable => reliablePacket != null;

            public AckablePacket(NotifyToken token)
            {
                this.token = token;
                reliablePacket = null;
            }

            public AckablePacket(ReliablePacket reliablePacket)
            {
                this.reliablePacket = reliablePacket;
                token = null;
            }

            public bool Equals(AckablePacket other)
            {
                return token == other.token &&
                    reliablePacket == other.reliablePacket;
            }
        }
        class ReliablePacket
        {
            public List<ushort> sequences = new List<ushort>();
            public ushort lastSequence;
            public bool acked;
            public readonly ByteBuffer buffer;
            public int length;
            public readonly ushort order;

            public void OnSend(ushort sequence)
            {
                sequences.Add(sequence);
                lastSequence = sequence;
            }

            public void OnAck()
            {
                acked = true;
                buffer.Release();
            }

            public ReliablePacket(ByteBuffer packet, int length, ushort order)
            {
                buffer = packet;
                this.length = length;
                this.order = order;
            }

            public override int GetHashCode()
            {
                return order;
            }
        }
        public struct ReliableReceived
        {
            public readonly ByteBuffer buffer;
            public readonly int length;

            public ReliableReceived(ByteBuffer buffer, int length)
            {
                this.buffer = buffer;
                this.length = length;
            }
        }
    }
}
