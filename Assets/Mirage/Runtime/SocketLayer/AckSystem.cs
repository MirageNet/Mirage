using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Mirage.SocketLayer
{
    internal class AckSystem
    {
        const int ACK_SEQUENCER_BITS = 16;
        // should be bit count of MAX_RECEIVE_QUEUE
        const int BUFFER_SIZE_BITS = 10;

        const int MASK_SIZE = sizeof(ulong) * 8;

        /// <summary>PacketType, sequence, ack sequannce, mask</summary>
        public const int HEADER_SIZE_NOTIFY = 5 + sizeof(ulong);
        /// <summary>PacketType, sequence, ack sequannce, mask, order</summary>
        public const int HEADER_SIZE_RELIABLE = 5 + sizeof(ulong) + 2;
        /// <summary>PacketType, ack sequannce, mask</summary>
        public const int HEADER_SIZE_ACK = 3 + sizeof(ulong);

        //public readonly Sequencer ackSequencer = new Sequencer(ACK_SEQUENCER_BITS);

        readonly RingBuffer<AckablePacket> sentAckablePackets = new RingBuffer<AckablePacket>(BUFFER_SIZE_BITS);
        readonly Sequencer reliableOrder = new Sequencer(BUFFER_SIZE_BITS);


        readonly RingBuffer<ReliableReceived> reliableReceive = new RingBuffer<ReliableReceived>(BUFFER_SIZE_BITS);

        // temp list for resending when processing sentqueue
        readonly HashSet<ReliablePacket> toResend = new HashSet<ReliablePacket>();

        public readonly int MTU;

        readonly IRawConnection connection;
        readonly ITime time;
        readonly float ackTimeout;
        /// <summary>how many empty acks to send</summary>
        readonly int emptyAckLimit;
        readonly int receivesBeforeEmpty;
        private readonly BufferPool bufferPool;

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

        ReliableReceived nextReliable;
        int nextReliableOffset;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="ackTimeout">how long after last send before sending empty ack</param>
        /// <param name="time"></param>
        public AckSystem(IRawConnection connection, Config config, ITime time, BufferPool bufferPool)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            this.connection = connection;
            this.time = time;
            this.bufferPool = bufferPool;

            ackTimeout = config.TimeBeforeEmptyAck;
            emptyAckLimit = config.EmptyAckLimit;
            receivesBeforeEmpty = config.ReceivesBeforeEmptyAck;
            MTU = config.Mtu;

            OnSend();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns>True if there is a new reliable message</returns>
        public bool NextReliablePacket(out ArraySegment<byte> message)
        {
            // todo return ReliableReceived (byte buffer and length) instead of segment and have consumer release buffer
            // add static method that gets message from bytebuffer?

            if (nextReliable.buffer == null)
            {
                if (reliableReceive.TryDequeue(out ReliableReceived byteBuffer))
                {
                    nextReliable = byteBuffer;
                    nextReliableOffset = 0;
                }
                else
                {
                    message = default;
                    return false;
                }
            }

            ushort length = ByteUtils.ReadUShort(nextReliable.buffer.array, ref nextReliableOffset);
            message = new ArraySegment<byte>(nextReliable.buffer.array, nextReliableOffset, length);
            nextReliableOffset += length;

            if (nextReliableOffset == nextReliable.length)
            {
                // todo dont release here, instead return bytebuffer
                nextReliable.buffer.Release();
                nextReliable = default;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void OnSend()
        {
            emptyAckCount++;
            lastSentAck = LatestAckSequence;
            lastSentTime = time.Now;
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

        private void SendAck()
        {
            using (ByteBuffer final = bufferPool.Take())
            {
                int offset = 0;

                ByteUtils.WriteByte(final.array, ref offset, (byte)PacketType.Ack);

                ByteUtils.WriteUShort(final.array, ref offset, LatestAckSequence);
                ByteUtils.WriteULong(final.array, ref offset, AckMask);

                connection.SendRaw(final.array, offset);
            }
            OnSend();
        }

        public INotifyToken SendNotify(byte[] packet)
        {
            // todo batch nofity?
            if (packet.Length + HEADER_SIZE_NOTIFY > MTU)
            {
                throw new IndexOutOfRangeException($"Message is bigger than MTU, max Notify message size is {MTU - HEADER_SIZE_NOTIFY}");
            }
            if (sentAckablePackets.IsFull)
            {
                throw new InvalidOperationException("Sent queue is full");
            }

            // todo use pool to stop allocations
            var token = new NotifyToken();
            ushort sequence = (ushort)sentAckablePackets.Enqueue(new AckablePacket(token));

            // todo check packet size is within MTU
            // todo use pool to stop allocations
            byte[] final = new byte[packet.Length + HEADER_SIZE_NOTIFY];
            Buffer.BlockCopy(packet, 0, final, HEADER_SIZE_NOTIFY, packet.Length);

            int offset = 0;

            ByteUtils.WriteByte(final, ref offset, (byte)PacketType.Notify);

            ByteUtils.WriteUShort(final, ref offset, sequence);
            ByteUtils.WriteUShort(final, ref offset, LatestAckSequence);
            ByteUtils.WriteULong(final, ref offset, AckMask);


            connection.SendRaw(final, final.Length);
            OnSend();

            return token;
        }


        public void SendReliable(byte[] message)
        {
            if (message.Length + HEADER_SIZE_RELIABLE + 2 > MTU)
            {
                throw new IndexOutOfRangeException($"Message is bigger than MTU, max Reliable message size is {MTU - HEADER_SIZE_RELIABLE - 2}");
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

            int msgLength = message.Length + 2;
            int batchLength = nextBatch.length;
            if (batchLength + msgLength > MTU)
            {
                // if full, send and create new
                SendReliablePacket(nextBatch);

                ushort order = (ushort)reliableOrder.Next();
                nextBatch = CreateReliableBuffer(order);
            }

            AddToBatch(nextBatch, message);
        }

        private ReliablePacket CreateReliableBuffer(ushort order)
        {

            ByteBuffer final = bufferPool.Take();

            int offset = 0;
            ByteUtils.WriteByte(final.array, ref offset, (byte)PacketType.Reliable);

            offset = 13;
            ByteUtils.WriteUShort(final.array, ref offset, order);

            return new ReliablePacket(final, HEADER_SIZE_RELIABLE, order);
        }
        static void AddToBatch(ReliablePacket packet, byte[] message)
        {
            byte[] array = packet.buffer.array;
            int offset = packet.length;

            int msgLength = message.Length;
            ByteUtils.WriteUShort(array, ref offset, (ushort)msgLength);
            Buffer.BlockCopy(message, 0, array, offset, msgLength);
            offset += msgLength;

            packet.length = offset;
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

            connection.SendRaw(final, reliable.length);
            OnSend();
        }


        /// <summary>
        /// Receives incoming nofity packet
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

            var segment = new ArraySegment<byte>(packet, HEADER_SIZE_NOTIFY, length - HEADER_SIZE_NOTIFY);
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
            else
            {
                ReliableReceived existing = reliableReceive[reliableSequence];
                if (existing.buffer == null)
                {
                    // new packet
                    ByteBuffer savedPacket = bufferPool.Take();
                    int bufferLength = length - HEADER_SIZE_RELIABLE;
                    Buffer.BlockCopy(packet, HEADER_SIZE_RELIABLE, savedPacket.array, 0, bufferLength);
                    reliableReceive.InsertAt(reliableSequence, new ReliableReceived(savedPacket, bufferLength));
                }
                return;
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
            ulong oldMask = AckMask;
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
                if (ackable.Equals(default)) { continue; }

                // if we have already ackeds this, just remove it
                if (ackable.IsReliable && ackable.reliablePacket.acked)
                {
                    // todo this should never happen, can we remove this?
                    sentAckablePackets.RemoveAt(ackableSequence);
                    continue;
                }
                int distance = (int)sentAckablePackets.Sequencer.Distance(sequence, ackableSequence);

                // negative distance means next is sent after last ack, so nothing to ack yet
                // no chance for it to be acked yet, so do nothing
                if (distance < 0)
                    continue;


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
        struct ReliableReceived
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
