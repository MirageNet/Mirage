using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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

    struct SentPacket
    {
        public uint Sequence;
        public NotifyToken token;
        public byte[] buffer;

        public static SentPacket Reliable(byte[] packet, uint sequence) => new SentPacket { buffer = packet, Sequence = sequence };
        public static SentPacket Notify(NotifyToken token, uint sequence) => new SentPacket { token = token, Sequence = sequence };
    }
    struct ReceivedPacket
    {
        public uint sequence;
        public byte[] buffer;
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

        const int MAX_SENT_QUEUE = 512;
        const int MAX_RECEIVE_QUEUE = 256;
        public const int HEADER_SIZE_NOTIFY = 9;
        public const int HEADER_SIZE_RELIABLE = 10;
        public const int HEADER_SIZE_ACK = 7;
        public readonly Sequencer ackSequencer = new Sequencer(16);
        // send and receive need different sequencers
        public readonly Sequencer reliableSendSequencer = new Sequencer(8);
        // received sequencer is to release messages in order to high level
        public readonly Sequencer reliableReceiveSequencer = new Sequencer(8);

        readonly IRawConnection connection;
        readonly Time time;
        readonly float ackTimeout;

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

        /// <summary>
        /// Reiable needs its own sequence so that message can be ordered
        /// </summary>
        int ReliableSentSequence;
        /// <summary>
        /// next received packet to be given to datahandler
        /// <para>used for Reliable Ordered messages</para>
        /// </summary>
        uint ReliableReceiveSequence;

        readonly Queue<SentPacket> sentPackets = new Queue<SentPacket>(MAX_SENT_QUEUE);
        /// <summary>reliable ordered queue</summary>
        readonly byte[][] receivedPackets = new byte[MAX_RECEIVE_QUEUE][];

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="ackTimeout">how long after last send before sending empty ack</param>
        /// <param name="time"></param>
        public AckSystem(IRawConnection connection, float ackTimeout, Time time)
        {
            this.connection = connection;
            this.time = time;
            this.ackTimeout = ackTimeout;

            // set received to first sequence
            // this means that it will always be 1 before first sent packet
            // so first receieved will have correct distance
            LatestAckSequence = (ushort)ackSequencer.Next();


            // first sent value is 0, which will be the reliableSentsSequencer first next
            // but receive increments after taking value, so we need to get the first expected id now, which is 0.
            // then the next value will be 1
            ReliableReceiveSequence = (ushort)reliableReceiveSequencer.Next();

            SetSendTime();
        }

        public bool NextReliablePacket(out byte[] buffer)
        {
            buffer = receivedPackets[ReliableReceiveSequence];
            // if next packet exists then return it
            if (buffer != null)
            {
                // clear it and increment sequence 
                receivedPackets[ReliableReceiveSequence] = null;
                ReliableReceiveSequence = (uint)reliableReceiveSequencer.Next();
                return true;
            }
            else
            {
                return false;
            }
        }

        void SetSendTime()
        {
            lastSentTime = time.Now;
        }
        public void Update()
        {
            // todo send ack if not recently been sent
            // ack only packet sent if no other sent within last frame
            if (lastSentTime + ackTimeout < time.Now)
            {
                // send ack
                SendAck();
            }
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
            SetSendTime();
        }

        public INotifyToken SendNotify(byte[] packet)
        {
            if (sentPackets.Count >= MAX_SENT_QUEUE)
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
            SetSendTime();

            // todo use pool to stop allocations
            var token = new NotifyToken(sequence);
            sentPackets.Enqueue(SentPacket.Notify(token, sequence));
            return token;
        }

        public void SendReliable(byte[] packet)
        {
            if (sentPackets.Count >= MAX_SENT_QUEUE)
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
            ByteUtils.WriteByte(final, ref offset, (byte)reliableSendSequencer.Next());

            // todo replace assert with log
            Assert.AreEqual(offset, HEADER_SIZE_RELIABLE);

            connection.SendRaw(final, final.Length);
            SetSendTime();


            sentPackets.Enqueue(SentPacket.Reliable(packet, sequence));
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

            SetReceivedNumbers(sequence, distance);
            CheckSentQueue(ackSequence, ackMask);

            var segment = new ArraySegment<byte>(packet, HEADER_SIZE_NOTIFY, packet.Length - HEADER_SIZE_NOTIFY);
            return segment;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packet"></param>
        /// <returns>true if there are ordered message to read</returns>
        public bool ReceiveReliable(byte[] packet)
        {
            // start at 1 to skip packet type
            int offset = 1;

            ushort sequence = ByteUtils.ReadUShort(packet, ref offset);
            ushort ackSequence = ByteUtils.ReadUShort(packet, ref offset);
            uint ackMask = ByteUtils.ReadUInt(packet, ref offset);
            byte reliableSequence = ByteUtils.ReadByte(packet, ref offset);

            int distance = (int)ackSequencer.Distance(sequence, LatestAckSequence);
            long distanceFromReliable = reliableReceiveSequencer.Distance(reliableSequence, ReliableReceiveSequence);

            // duplicate or arrived late
            if (distance <= 0)
            {
                // check distance from reliable index

                // if still negative it is duplicate
                // 0 is next packet, so is not duplicate.
                if (distanceFromReliable < 0) { return false; }
                // if positive it just arrived late and we can still use it
            }

            SetReceivedNumbers(sequence, distance);
            CheckSentQueue(ackSequence, ackMask);

            // copy to new array because packet will return to buffer
            //todo use buffer to reduce allocations
            byte[] savedPacket = new byte[packet.Length - HEADER_SIZE_RELIABLE];
            Buffer.BlockCopy(packet, HEADER_SIZE_RELIABLE, savedPacket, 0, savedPacket.Length);
            receivedPackets[reliableSequence] = savedPacket;

            // if distance is 0 then it is the next packet
            return distanceFromReliable == 0;
        }
        private void SetReceivedNumbers(ushort latest, int distance)
        {
            // shift mask by distance, then add 1
            // eg distance = 2
            // this will mean mask will be ..01
            // which means that 1 packet was missed
            AckMask = (AckMask << distance) | 1;

            LatestAckSequence = latest;
        }
        internal void ReceiveNotifyAck(byte[] packet)
        {
            // start at 1 to skip packet type
            int offset = 1;

            ushort ackSequence = ByteUtils.ReadUShort(packet, ref offset);
            uint ackMask = ByteUtils.ReadUInt(packet, ref offset);

            CheckSentQueue(ackSequence, ackMask);
        }

        private void ResendReliable(SentPacket packet)
        {
            byte[] final = packet.buffer;
            // skip writing type and reliable sequence, they will be in buffer from last time it was sent
            int offset = 1;

            // write new sequence and acks
            ushort sequence = (ushort)ackSequencer.Next();
            ByteUtils.WriteUShort(final, ref offset, sequence);
            ByteUtils.WriteUShort(final, ref offset, LatestAckSequence);
            ByteUtils.WriteUInt(final, ref offset, AckMask);

            connection.SendRaw(final, final.Length);
            SetSendTime();

            sentPackets.Enqueue(SentPacket.Reliable(final, sequence));
        }

        private void CheckSentQueue(ushort sequence, uint mask)
        {
            while (sentPackets.Count > 0)
            {
                SentPacket sentPacket = sentPackets.Peek();

                int distance = (int)ackSequencer.Distance(sequence, sentPacket.Sequence);

                // negative distance means next is sent after last ack, so nothing to ack yet
                if (distance < 0)
                    return;

                // positive distance means it should have been acked, or mark it as lost
                sentPackets.Dequeue();

                // if distance above size then it is outside of mask, so set as lost
                bool lost = OutsideOfMask(distance) || NotInMask(distance, mask);


                if (sentPacket.token != null)
                {
                    sentPacket.token.Notify(!lost);
                }
                // not notify, and lost
                else if (lost)
                {
                    ResendReliable(sentPacket);
                }
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
    }
}
