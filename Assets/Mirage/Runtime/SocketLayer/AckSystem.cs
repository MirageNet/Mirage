using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Assertions;

namespace Mirage.SocketLayer
{
    struct AckHeader
    {
        public ushort ackSequence;
        public uint ackMask;

        /// <summary>
        /// should packet be used and sent higher up to mirage
        /// <para>It could be invalid because it as a duplicate packet or arrived late</para>
        /// </summary>
        public bool isValid;

        public static AckHeader Invalid() => new AckHeader { isValid = false };
    }

    struct SentNotify
    {
        public uint sequence;
        public NotifyToken token;

        public SentNotify(NotifyToken token, ushort sequence)
        {
            this.token = token;
            this.sequence = sequence;
        }
    }

    /// <summary>
    /// Object returned from <see cref="NotifySystem.Send(byte[])"/> with events for when packet is Lost or Delivered
    /// </summary>
    public interface INotifyToken
    {
        event Action Delivered;
        event Action Lost;
    }

    /// <summary>
    /// Object returned from <see cref="NotifySystem.Send(byte[])"/> with events for when packet is Lost or Delivered
    /// </summary>
    public class NotifyToken : INotifyToken
    {
        public event Action Delivered;
        public event Action Lost;

        bool notified;
        internal readonly ushort Sequence;

        internal NotifyToken(ushort Sequence)
        {
            this.Sequence = Sequence;
        }

        internal void Notify(bool delivered)
        {
            if (notified) throw new InvalidOperationException("this token as already been notified");
            notified = true;

            if (delivered)
                Delivered?.Invoke();
            else
                Lost?.Invoke();
        }
    }

    internal class AckSystem
    {
        // todo https://gafferongames.com/post/reliable_ordered_messages/

        const bool VerboseLogging = false;

        const int MAX_SENT_QUEUE = 1024;
        const int MAX_RECEIVE_QUEUE = 1024;

        const int ACK_SEQUENCER_BITS = 16;
        // should be bit count of MAX_RECEIVE_QUEUE
        const int RELIABLE_SEQUENCER_BITS = 10;

        public const int HEADER_SIZE_NOTIFY = 9;
        public const int HEADER_SIZE_RELIABLE = 11;
        public const int HEADER_SIZE_ACK = 7;
        public readonly Sequencer ackSequencer = new Sequencer(ACK_SEQUENCER_BITS);

        readonly SentBuffer reliableSend = new SentBuffer();
        readonly ReceiveBuffer reliableReceive = new ReceiveBuffer();


        readonly IRawConnection connection;
        readonly ITime time;
        readonly float ackTimeout;
        /// <summary>how many empty acks to send</summary>
        readonly int emptyAckLimit = 20;

        /// <summary>
        /// most recent sequence received
        /// <para>will be sent with next message</para>
        /// </summary>
        ushort LatestAckSequence;
        /// <summary>
        /// mask of recent sequences received
        /// <para>will be sent with next message</para>
        /// </summary>
        uint AckMask;

        float lastSentTime;
        int emptyAckCount = 0;

        readonly Queue<SentNotify> sentNotifies = new Queue<SentNotify>(MAX_SENT_QUEUE);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="ackTimeout">how long after last send before sending empty ack</param>
        /// <param name="time"></param>
        public AckSystem(IRawConnection connection, float ackTimeout, ITime time)
        {
            this.connection = connection;
            this.time = time;
            this.ackTimeout = ackTimeout;

            // set received to first sequence
            // this means that it will always be 1 before first sent packet
            // so first receieved will have correct distance
            LatestAckSequence = (ushort)ackSequencer.Next();

            OnNormalSend();
        }

        public bool NextReliablePacket(out byte[] buffer)
        {
            return reliableReceive.PopNextReceive(out buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void OnSendEmptyAck()
        {
            emptyAckCount++;
            SetSendTime();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void OnNormalSend()
        {
            emptyAckCount = 0;
            SetSendTime();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void SetSendTime()
        {
            lastSentTime = time.Now;
        }

        public void Update()
        {
            // todo send ack if not recently been sent
            // ack only packet sent if no other sent within last frame
            if (ShouldSendEmptyAck() && TimeToSendAck())
            {
                if (VerboseLogging) { Debug.LogWarning("empty acks"); }
                // send ack
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
            bool shouldSend = emptyAckCount < emptyAckLimit;
            if (!shouldSend)
            {
                if (VerboseLogging) { Debug.LogWarning("no more empty acks"); }
            }
            return shouldSend;
        }

        private void SendAck()
        {
            // todo use pool to stop allocations
            byte[] final = new byte[HEADER_SIZE_ACK];
            int offset = 0;

            ByteUtils.WriteByte(final, ref offset, (byte)PacketType.Ack);

            ByteUtils.WriteUShort(final, ref offset, LatestAckSequence);
            ByteUtils.WriteUInt(final, ref offset, AckMask);

            // todo replace assert with log
            Assert.AreEqual(offset, HEADER_SIZE_ACK);

            connection.SendRaw(final, final.Length);
            OnSendEmptyAck();
        }

        public INotifyToken SendNotify(byte[] packet)
        {
            if (sentNotifies.Count >= MAX_SENT_QUEUE)
            {
                throw new InvalidOperationException("Sent queue is full");
            }

            // todo check packet size is within MTU
            // todo use pool to stop allocations
            byte[] final = new byte[packet.Length + HEADER_SIZE_NOTIFY];
            Buffer.BlockCopy(packet, 0, final, HEADER_SIZE_NOTIFY, packet.Length);

            int offset = 0;

            ByteUtils.WriteByte(final, ref offset, (byte)PacketType.Notify);

            ushort sequence = (ushort)ackSequencer.Next();
            ByteUtils.WriteUShort(final, ref offset, sequence);
            ByteUtils.WriteUShort(final, ref offset, LatestAckSequence);
            ByteUtils.WriteUInt(final, ref offset, AckMask);

            // todo replace assert with log
            Assert.AreEqual(offset, HEADER_SIZE_NOTIFY);

            connection.SendRaw(final, final.Length);
            OnNormalSend();

            // todo use pool to stop allocations
            var token = new NotifyToken(sequence);
            sentNotifies.Enqueue(new SentNotify(token, sequence));
            return token;
        }

        public void SendReliable(byte[] packet)
        {
            if (reliableSend.IsFull())
            {
                throw new InvalidOperationException("Sent queue is full");
            }


            // todo check packet size is within MTU
            // todo use pool to stop allocations
            byte[] final = new byte[packet.Length + HEADER_SIZE_RELIABLE];
            Buffer.BlockCopy(packet, 0, final, HEADER_SIZE_RELIABLE, packet.Length);

            int offset = 0;

            ByteUtils.WriteByte(final, ref offset, (byte)PacketType.Reliable);

            ushort sequence = (ushort)ackSequencer.Next();
            ByteUtils.WriteUShort(final, ref offset, sequence);
            ByteUtils.WriteUShort(final, ref offset, LatestAckSequence);
            ByteUtils.WriteUInt(final, ref offset, AckMask);
            ushort writeIndex = (ushort)reliableSend.NextWrite;
            ByteUtils.WriteUShort(final, ref offset, writeIndex);

            if (VerboseLogging) { Debug.LogWarning($"SendReliable {writeIndex}"); }

            // todo replace assert with log
            Assert.AreEqual(offset, HEADER_SIZE_RELIABLE);

            connection.SendRaw(final, final.Length);
            OnNormalSend();

            // enqueue the final buffer so that if it needs to be resent the sendSequance will still be in there
            reliableSend.AddNext(sequence, final);
        }


        public ArraySegment<byte> ReceiveNotify(byte[] packet)
        {
            // start at 1 to skip packet type
            int offset = 1;

            ushort sequence = ByteUtils.ReadUShort(packet, ref offset);
            ushort ackSequence = ByteUtils.ReadUShort(packet, ref offset);
            uint ackMask = ByteUtils.ReadUInt(packet, ref offset);

            int distance = (int)ackSequencer.Distance(sequence, LatestAckSequence);

            // duplicate or arrived late
            if (distance <= 0) { return default; }

            SetAckValues(sequence, distance);
            CheckSentQueue(ackSequence, ackMask);

            var segment = new ArraySegment<byte>(packet, HEADER_SIZE_NOTIFY, packet.Length - HEADER_SIZE_NOTIFY);
            return segment;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packet"></param>
        /// <returns>true if there are ordered message to read</returns>
        public (bool valid, byte[] nextInOrder, int offsetInBuffer) ReceiveReliable(byte[] packet)
        {
            // start at 1 to skip packet type
            int offset = 1;

            ushort sequence = ByteUtils.ReadUShort(packet, ref offset);
            ushort ackSequence = ByteUtils.ReadUShort(packet, ref offset);
            uint ackMask = ByteUtils.ReadUInt(packet, ref offset);
            ushort reliableSequence = ByteUtils.ReadUShort(packet, ref offset);
            long distance = ackSequencer.Distance(sequence, LatestAckSequence);


            // checks acks, late message are allowed for reliable
            // but only update lastest if later than current

            SetAckValues(sequence, distance);
            CheckSentQueue(ackSequence, ackMask);

            ushort readIndex = (ushort)reliableReceive.NextRead;
            long reliableDistance = reliableReceive.Sequencer.Distance(reliableSequence, readIndex);

            if (VerboseLogging) { Debug.Log($"ReceiveReliable [sequence:{sequence}, distance:{distance}, reliableSequence:{reliableSequence} reliableDistance{reliableDistance}]"); }


            if (reliableDistance < 0)
            {
                // old packet
                return (false, default, default);
            }
            else if (reliableDistance == 0)
            {
                reliableReceive.MoveNext();
                // next packet
                return (true, packet, offset);
            }
            else
            {
                // new packet
                byte[] savedPacket = new byte[packet.Length - HEADER_SIZE_RELIABLE];
                Buffer.BlockCopy(packet, HEADER_SIZE_RELIABLE, savedPacket, 0, savedPacket.Length);
                reliableReceive.Add(reliableSequence, savedPacket);
                return (false, default, default);
            }
        }
        private void SetAckValues(ushort sequence, long distance)
        {
            uint oldMask = AckMask;
            if (distance > 0)
            {
                // shift mask by distance, then add 1
                // eg distance = 2
                // this will mean mask will be ..01
                // which means that 1 packet was missed
                AckMask = (AckMask << (int)distance) | 1;
                LatestAckSequence = sequence;
            }
            else
            {
                uint newAck = 1u << -(int)distance;
                AckMask |= newAck;
            }
            if (VerboseLogging) { Console.WriteLine($"Ackmask distance:{distance}\n{Convert.ToString(oldMask, 2).PadLeft(32, '0')}\n{Convert.ToString(AckMask, 2).PadLeft(32, '0')}"); }
        }

        internal void ReceiveAck(byte[] packet)
        {
            // start at 1 to skip packet type
            int offset = 1;

            ushort ackSequence = ByteUtils.ReadUShort(packet, ref offset);
            uint ackMask = ByteUtils.ReadUInt(packet, ref offset);

            CheckSentQueue(ackSequence, ackMask);
        }

        private void ResendReliable(SentBuffer.Sent sent)
        {
            if (VerboseLogging)
            {
                int debugOffset = 9;
                Debug.LogWarning($"ResendReliable {ByteUtils.ReadUShort(sent.buffer, ref debugOffset)}, full {BitConverter.ToString(sent.buffer)}");
            }

            byte[] final = sent.buffer;
            // skip writing type and reliable sequence, they will be in buffer from last time it was sent
            int offset = 1;

            // write new sequence and acks
            ushort sequence = (ushort)ackSequencer.Next();
            ByteUtils.WriteUShort(final, ref offset, sequence);
            ByteUtils.WriteUShort(final, ref offset, LatestAckSequence);
            ByteUtils.WriteUInt(final, ref offset, AckMask);

            connection.SendRaw(final, final.Length);
            OnNormalSend();

            sent.sequences.Add(sequence);
        }

        private void CheckSentQueue(ushort sequence, uint mask)
        {
            if (VerboseLogging) { Debug.Log($"Ack mask {Convert.ToString(mask, 2)}"); }
            CheckSentNotify(sequence, mask);
            CheckSentReliable(sequence, mask);
        }

        private void CheckSentReliable(ushort sequence, uint mask)
        {
            foreach (SentBuffer.Sent sent in reliableSend)
            {
                if (VerboseLogging)
                {
                    if (sent == null)
                    {
                        Debug.LogWarning($"Checking:{sequence} NULL");
                    }
                    else
                    {
                        Debug.LogWarning($"Checking:{sequence} sent:{sent.index} [{string.Join(",", sent.sequences)}]");
                    }
                }

                // not existing or already removed
                if (sent == null || sent.buffer == null) { continue; }

                bool received = false;
                bool hasPendingSends = false;
                foreach (ushort sentSequence in sent.sequences)
                {
                    int distance = (int)reliableSend.Sequencer.Distance(sequence, sentSequence);

                    // negative distance means next is sent after last ack, so nothing to ack yet
                    if (distance < 0)
                    {
                        hasPendingSends = true;
                        continue;
                    }

                    // if distance above size then it is outside of mask, so set as lost
                    bool lost = OutsideOfMask(distance) || NotInMask(distance, mask);
                    if (!lost)
                    {
                        received = true;
                    }
                }

                if (received)
                {
                    if (VerboseLogging) Debug.LogWarning($"Removed Acked: {sent.index}");
                    // remove
                    sent.buffer = null;
                    sent.sequences.Clear();
                }
                else if (!hasPendingSends)
                {
                    // no pending, resend now
                    ResendReliable(sent);
                }
            }
        }

        private void CheckSentNotify(ushort sequence, uint mask)
        {
            while (sentNotifies.Count > 0)
            {
                SentNotify sent = sentNotifies.Peek();

                int distance = (int)ackSequencer.Distance(sequence, sent.sequence);


                // negative distance means next is sent after last ack, so nothing to ack yet
                if (distance < 0)
                    return;

                // positive distance means it should have been acked, or mark it as lost
                sentNotifies.Dequeue();

                // if distance above size then it is outside of mask, so set as lost
                bool lost = OutsideOfMask(distance) || NotInMask(distance, mask);
                sent.token.Notify(!lost);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool OutsideOfMask(int distance)
        {
            const int maskSize = sizeof(uint) * 8;
            return distance > maskSize;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool NotInMask(int distance, uint receivedMask)
        {
            uint ackBit = 1u << distance;
            return (receivedMask & ackBit) == 0u;
        }

        class ReceiveBuffer
        {
            public readonly Sequencer Sequencer = new Sequencer(RELIABLE_SEQUENCER_BITS);
            /// <summary>
            /// oldest
            /// </summary>
            ulong read;

            public ulong NextRead => read;


            /// <summary>reliable ordered queue</summary>
            readonly byte[][] receivedPackets = new byte[MAX_RECEIVE_QUEUE][];

            /// <summary>
            /// Get next received buffer in correct order
            /// </summary>
            /// <param name="buffer"></param>
            /// <returns>valid receive</returns>
            public bool PopNextReceive(out byte[] buffer)
            {
                buffer = receivedPackets[read];
                if (buffer != null)
                {
                    if (VerboseLogging) { Debug.Log($"PopNextReceive [reliableSequence:{read}]"); }
                    receivedPackets[read] = null;
                    read = Sequencer.NextAfter(read);
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public void MoveNext()
            {
                read = Sequencer.NextAfter(read);
            }

            public void Add(ushort sequence, byte[] packet)
            {
                // todo check if sequence is old or new???

                if (receivedPackets[sequence] != null)
                {
                    if (VerboseLogging) { Debug.LogWarning($"Already in buffer: {sequence}"); }
                    return;
                }

                receivedPackets[sequence] = packet;
            }
        }
        class SentBuffer : IEnumerable<SentBuffer.Sent>
        {
            public class Sent
            {
                /// <summary>
                /// could be sent multiple times, this is the list of sequences it is sent as
                /// </summary>
                public List<ushort> sequences = new List<ushort>();

                public byte[] buffer;
                // debug index
                public ulong index;
            }
            public readonly Sequencer Sequencer = new Sequencer(RELIABLE_SEQUENCER_BITS);

            /// <summary>
            /// oldest
            /// </summary>
            ulong read;

            /// <summary>
            /// newest
            /// </summary>
            ulong write;

            /// <summary>reliable ordered queue</summary>
            readonly Sent[] sentPackets = new Sent[MAX_RECEIVE_QUEUE];

            public ulong NextWrite => write;

            public bool IsFull()
            {
                long dist = Sequencer.Distance(write, read);
                return dist == -1;
            }
            public bool IsEmpty()
            {
                long dist = Sequencer.Distance(write, read);
                return dist == 0;
            }

            public void AddNext(ushort sequence, byte[] packet)
            {
                // todo fix this, need to check sequence is not past read
                long headTailDistance = Sequencer.Distance(write, read);
                if (headTailDistance == -1)
                {
                    Debug.LogError($"Buffer full!");
                    return;
                }

                Sent next = sentPackets[write];
                if (next == null)
                {
                    next = new Sent();
                    next.index = write;
                    sentPackets[write] = next;
                }

                next.buffer = packet;
                next.sequences.Clear();
                next.sequences.Add(sequence);
                if (VerboseLogging) { Debug.LogWarning($"SentBuffer index:{write} sequence:{sequence}"); }

                write = Sequencer.NextAfter(write);
            }

            public IEnumerator<Sent> GetEnumerator() => new Enumerator(this);
            IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);

            public struct Enumerator : IEnumerator<Sent>
            {
                readonly SentBuffer buffer;
                ulong index;
                bool first;
                public Sent Current => buffer.sentPackets[index];

                public Enumerator(SentBuffer buffer)
                {
                    this.buffer = buffer;
                    index = buffer.read;
                    first = true;
                }

                public bool MoveNext()
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        index = buffer.Sequencer.NextAfter(index);
                    }

                    /*
                    i = 99, write = 110,
                    distance = 110 - 99 = 11  => go next

                    i = 110, write = 110,
                    distance = 0  => stop

                    i = 111, write = 110,
                    distance = -1  => stop
                     */
                    if (buffer.Sequencer.Distance(buffer.write, index) > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                public void Reset() => index = buffer.read;
                object IEnumerator.Current => Current;
                public void Dispose()
                {
                    // nothing to dispose
                }
            }
        }
    }
}
