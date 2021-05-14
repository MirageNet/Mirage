using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Assertions;

namespace Mirage.SocketLayer
{
    class AckablePacket
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
    }
    class ReliablePacket
    {
        public List<ushort> sequences = new List<ushort>();
        public bool acked;
        public readonly byte[] packet;
        public readonly ushort order;

        public ReliablePacket(byte[] packet, ushort order)
        {
            this.packet = packet;
            this.order = order;
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

        public static bool VerboseLogging = false;
        public static bool LogToFunction = false;
        public static Action<string> Log;

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

        readonly RingBuffer<byte[]> reliableReceive = new RingBuffer<byte[]>(BUFFER_SIZE_BITS);

        // temp list for resending when processing sentqueue
        readonly HashSet<ReliablePacket> toResend = new HashSet<ReliablePacket>();


        readonly IRawConnection connection;
        readonly ITime time;
        readonly float ackTimeout;
        /// <summary>how many empty acks to send</summary>
        readonly int emptyAckLimit = 20;
        readonly int receivesBeforeEmpty = 8;

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

            //// set received to first sequence
            //// this means that it will always be 1 before first sent packet
            //// so first receieved will have correct distance
            //LatestAckSequence = (ushort)ackSequencer.Next();

            OnNormalSend();
        }

        public bool NextReliablePacket(out byte[] buffer)
        {
            return reliableReceive.TryDequeue(out buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void OnSendEmptyAck()
        {
            emptyAckCount++;
            lastSentAck = LatestAckSequence;
            SetSendTime();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void OnNormalSend()
        {
            emptyAckCount = 0;
            lastSentAck = LatestAckSequence;
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
                // send ack
                SendAck();
            }
        }

        void CheckSendEmptyAck()
        {
            long distance = sentAckablePackets.Sequencer.Distance(LatestAckSequence, lastSentAck);
            if (VerboseLogging) { Debug.Log($"distance To lastSentAck:{distance}"); }
            if (distance > receivesBeforeEmpty)
            {
                SendAck();
                // reset empty ack count beecause this is a new ack that should be received
                emptyAckCount = 0;
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
            if (VerboseLogging) { Debug.LogWarning($"empty acks, count:{emptyAckCount}"); }

            // todo use pool to stop allocations
            byte[] final = new byte[HEADER_SIZE_ACK];
            int offset = 0;

            ByteUtils.WriteByte(final, ref offset, (byte)PacketType.Ack);

            ByteUtils.WriteUShort(final, ref offset, LatestAckSequence);
            ByteUtils.WriteULong(final, ref offset, AckMask);

            // todo replace assert with log
            Assert.AreEqual(offset, HEADER_SIZE_ACK);

            connection.SendRaw(final, final.Length);
            OnSendEmptyAck();
        }

        public INotifyToken SendNotify(byte[] packet)
        {
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

            // todo replace assert with log
            Assert.AreEqual(offset, HEADER_SIZE_NOTIFY);

            connection.SendRaw(final, final.Length);
            OnNormalSend();

            return token;
        }


        public void SendReliable(byte[] packet)
        {
            if (sentAckablePackets.IsFull)
            {
                throw new InvalidOperationException("Sent queue is full");
            }

            ulong order = reliableOrder.Next();
            var reliablePacket = new ReliablePacket(packet, (ushort)order);
            SendReliablePacket(reliablePacket);
        }
        private void SendReliablePacket(ReliablePacket reliable)
        {
            if (reliable.acked) { return; }

            ushort sequence = (ushort)sentAckablePackets.Enqueue(new AckablePacket(reliable));

            byte[] packet = reliable.packet;
            // todo check packet size is within MTU
            // todo use pool to stop allocations
            byte[] final = new byte[packet.Length + HEADER_SIZE_RELIABLE];
            Buffer.BlockCopy(packet, 0, final, HEADER_SIZE_RELIABLE, packet.Length);

            int offset = 0;

            ByteUtils.WriteByte(final, ref offset, (byte)PacketType.Reliable);

            reliable.sequences.Add(sequence);
            ByteUtils.WriteUShort(final, ref offset, sequence);
            ByteUtils.WriteUShort(final, ref offset, LatestAckSequence);
            ByteUtils.WriteULong(final, ref offset, AckMask);
            ByteUtils.WriteUShort(final, ref offset, reliable.order);

            if (VerboseLogging) { Debug.LogWarning($"SendReliablePacket {reliable.order}"); }
            if (AckSystem.LogToFunction) { AckSystem.Log($"SendReliablePacket {reliable.order}"); }
            connection.SendRaw(final, final.Length);
            OnNormalSend();
        }

        /// <summary>
        /// Receives incoming nofity packet
        /// <para>Ignores duplicate or late packets</para>
        /// </summary>
        /// <param name="packet"></param>
        /// <returns>default or new packet to handle</returns>
        public ArraySegment<byte> ReceiveNotify(byte[] packet)
        {
            // start at 1 to skip packet type
            int offset = 1;

            ushort sequence = ByteUtils.ReadUShort(packet, ref offset);
            ushort ackSequence = ByteUtils.ReadUShort(packet, ref offset);
            ulong ackMask = ByteUtils.ReadULong(packet, ref offset);

            int distance = ProcessIncomingHeader(sequence, ackSequence, ackMask);

            // duplicate or arrived late
            if (distance <= 0) { return default; }

            var segment = new ArraySegment<byte>(packet, HEADER_SIZE_NOTIFY, packet.Length - HEADER_SIZE_NOTIFY);
            return segment;
        }


        /// <summary>
        /// </summary>
        /// <param name="packet"></param>
        /// <returns>true if there are ordered message to read</returns>
        public (bool valid, byte[] nextInOrder, int offsetInBuffer) ReceiveReliable(byte[] packet)
        {
            // start at 1 to skip packet type
            int offset = 1;

            ushort sequence = ByteUtils.ReadUShort(packet, ref offset);
            ushort ackSequence = ByteUtils.ReadUShort(packet, ref offset);
            ulong ackMask = ByteUtils.ReadULong(packet, ref offset);
            ushort reliableSequence = ByteUtils.ReadUShort(packet, ref offset);

            int distance = ProcessIncomingHeader(sequence, ackSequence, ackMask);

            // checks acks, late message are allowed for reliable
            // but only update lastest if later than current

            long reliableDistance = reliableReceive.DistanceToRead(reliableSequence);

            if (VerboseLogging) { Debug.Log($"ReceiveReliable [sequence:{sequence}, distance:{distance}, reliableSequence:{reliableSequence} reliableDistance{reliableDistance}]"); }
            if (AckSystem.LogToFunction) { AckSystem.Log($"ReceiveReliable [sequence:{sequence}, distance:{distance}, reliableSequence:{reliableSequence} reliableDistance{reliableDistance}]"); }


            if (reliableDistance < 0)
            {
                // old packet
                return (false, default, default);
            }
            else if (reliableDistance == 0)
            {
                reliableReceive.MoveReadOne();
                // next packet
                return (true, packet, offset);
            }
            else
            {
                // new packet
                byte[] savedPacket = new byte[packet.Length - HEADER_SIZE_RELIABLE];
                Buffer.BlockCopy(packet, HEADER_SIZE_RELIABLE, savedPacket, 0, savedPacket.Length);
                reliableReceive.InsertAt(reliableSequence, savedPacket);
                return (false, default, default);
            }
        }


        internal void ReceiveAck(byte[] packet)
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
                if (distance > 64)
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
                CheckSendEmptyAck();
            }
            else
            {
                int negativeDistnace = -(int)distance;

                // distance is too large to be shifted
                if (negativeDistnace > 64)
                    return;

                ulong newAck = 1ul << negativeDistnace;
                AckMask |= newAck;
            }
            if (VerboseLogging)
            {
                Console.WriteLine($"Ackmask distance:{distance}\n" +
                    $"{Convert.ToString((long)oldMask, 2)}\n" +
                     $"{Convert.ToString((long)AckMask, 2)}\n");
            }
        }

        private void CheckSentQueue(ushort sequence, ulong mask)
        {
            if (VerboseLogging) { Debug.Log($"Ack mask {Convert.ToString((long)mask, 2)}"); }

            // old sequence, nothing in buffer to ack/lost
            if (sentAckablePackets.DistanceToRead(sequence) < 0) { return; }

            foreach (RingBuffer<AckablePacket>.Kvp kvp in sentAckablePackets)
            {
                AckablePacket ackable = kvp.item;
                uint ackableSequence = kvp.sequence;

                // null if packet has been acked
                if (ackable == null) { continue; }

                // if we have already ackeds this, just remove it
                if (ackable.IsReliable && ackable.reliablePacket.acked)
                {
                    if (LogToFunction) Log($"Ackable Remove {ackableSequence} (previously acked)");
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
                    if (LogToFunction) Log($"Ackable Remove {ackableSequence} (Notify)");
                    sentAckablePackets.RemoveAt(ackableSequence);
                }
                else
                {
                    ReliablePacket reliablePacket = ackable.reliablePacket;
                    if (lost)
                    {
                        // we need to resend the packet if it has been possible for it to have been acked

                        bool needToResend = true;
                        foreach (ushort seq in reliablePacket.sequences)
                        {
                            // if any of the sequences are after sequence it has already been resent
                            if (sentAckablePackets.Sequencer.Distance(sequence, seq) < 0)
                            {
                                needToResend = false;
                            }
                        }

                        if (needToResend)
                            toResend.Add(reliablePacket);
                        else
                            if (LogToFunction) Log($"Ackable Not resend order:{reliablePacket.order} (already resent)");
                    }
                    else
                    {
                        reliablePacket.acked = true;
                        if (LogToFunction) Log($"Ackable acked order:{reliablePacket.order} [{sequence},{Convert.ToString((long)mask)}]");
                        foreach (ushort seq in reliablePacket.sequences)
                        {
                            sentAckablePackets.RemoveAt(seq);
                            if (LogToFunction) Log($"Ackable Remove {seq} via {ackableSequence}");
                        }
                    }
                }
            }

            sentAckablePackets.MoveReadToNextNonEmpty();

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
    }
    public class RingBuffer<T> : IEnumerable<RingBuffer<T>.Kvp> where T : class
    {
        public readonly Sequencer Sequencer;

        T[] buffer;
        // oldtest item
        uint read;
        // newest item
        uint write;

        public uint Read => read;
        public uint Write => write;

        public RingBuffer(int bitCount)
        {
            Sequencer = new Sequencer(bitCount);
            buffer = new T[1 << bitCount];
        }

        public bool IsFull => Sequencer.Distance(write, read) == -1;
        public long DistanceToRead(uint from)
        {
            return Sequencer.Distance(from, read);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns>sequance of written item</returns>
        public uint Enqueue(T item)
        {
            long dist = Sequencer.Distance(write, read);
            if (dist == -1) { throw new InvalidOperationException($"Buffer is full, write:{write} read:{read}"); }

            if (AckSystem.LogToFunction) { AckSystem.Log($"Enqueue: write:{write} read:{read}"); }
            buffer[write] = item;
            uint sequence = write;
            write = (uint)Sequencer.NextAfter(write);
            return sequence;
        }
        public bool TryDequeue(out T item)
        {
            item = buffer[read];
            if (item != null)
            {
                if (AckSystem.VerboseLogging) { Debug.LogWarning($"Dequeuing next {read}"); }
                if (AckSystem.LogToFunction) { AckSystem.Log($"Dequeuing next {read}"); }

                buffer[read] = null;
                read = (uint)Sequencer.NextAfter(read);

                return true;
            }
            else
            {
                return false;
            }
        }

        public void InsertAt(uint index, T item)
        {
            buffer[index] = item;
        }
        public void RemoveAt(uint index)
        {
            buffer[index] = null;
        }


        /// <summary>
        /// Moves read index to next non empty position
        /// <para>this is useful when removing items from buffer in random order.</para>
        /// <para>Will stop when write == read, or when next buffer item is not empty</para>
        /// </summary>
        public void MoveReadToNextNonEmpty()
        {
            uint oldRead = read;
            // if read == write, buffer is empty, dont move it
            // if buffer[read] is empty then read to next item
            while (write != read && buffer[read] == null)
            {
                read = (uint)Sequencer.NextAfter(read);
            }
            if (AckSystem.LogToFunction) { AckSystem.Log($"Moving Read from {oldRead} to {read}"); }
        }

        /// <summary>
        /// Moves read 1 index
        /// </summary>
        public void MoveReadOne()
        {
            if (AckSystem.VerboseLogging) { Debug.LogWarning($"Dequeuing(skip) next {read}"); }
            if (AckSystem.LogToFunction) { AckSystem.Log($"Dequeuing(skip) next {read}"); }


            read = (uint)Sequencer.NextAfter(read);
        }

        public IEnumerator<Kvp> GetEnumerator() => new Enumerator(this);
        IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);


        public struct Kvp
        {
            public uint sequence;
            public T item;

            public Kvp(uint sequence, T item) : this()
            {
                this.sequence = sequence;
                this.item = item;
            }
        }
        public struct Enumerator : IEnumerator<Kvp>
        {
            readonly RingBuffer<T> buffer;
            ulong nextIndex;
            Kvp current;

            public Kvp Current => current;

            public Enumerator(RingBuffer<T> buffer)
            {
                this.buffer = buffer;
                nextIndex = buffer.read;
                current = default;
            }

            public bool MoveNext()
            {
                /*
                 i = 99, write = 110,
                 distance = 110 - 99 = 11  => go next

                 i = 110, write = 110,
                 distance = 0  => stop

                 i = 111, write = 110,
                 distance = -1  => stop
                  */
                if (buffer.Sequencer.Distance(buffer.write, nextIndex) > 0)
                {
                    current = new Kvp((uint)nextIndex, buffer.buffer[nextIndex]);
                    nextIndex = buffer.Sequencer.NextAfter(nextIndex);

                    return true;
                }
                else
                {
                    return false;
                }
            }

            public void Reset() => nextIndex = buffer.read;
            object IEnumerator.Current => Current;
            public void Dispose()
            {
                // nothing to dispose
            }
        }
    }
}
