using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Mirage.SocketLayer
{
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
                Delivered.Invoke();
            else
                Lost.Invoke();
        }
    }

    /// <summary>
    /// keeps track of tokens sent and received 
    /// </summary>
    // todo add more docs
    internal class NotifySystem
    {
        readonly AckSystem ackSystem;

        const int MaxSentQueue = 512;
        readonly Queue<NotifyToken> sent = new Queue<NotifyToken>(MaxSentQueue);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="ackTimeout">how long after last send before sending empty ack, see <see cref="AckSystem.AckSystem(IRawConnection, float, Time)"/></param>
        /// <param name="time"></param>

        public NotifySystem(IRawConnection connection, float ackTimeout, Time time)
        {

        }

        public NotifySystem(AckSystem ackSystem)
        {
            this.ackSystem = ackSystem;
        }

        public INotifyToken Send(byte[] packet)
        {
            if (sent.Count >= MaxSentQueue)
            {
                throw new InvalidOperationException("Sent queue is full");
            }

            ushort sequence = ackSystem.Send(packet);

            // todo use pool to stop allocations
            var token = new NotifyToken(sequence);
            sent.Enqueue(token);
            return token;
        }


        internal void ReceiveAck(byte[] array)
        {
            ReceivedPacket received = ackSystem.ReceiveAck(array);

            CheckSentQueue(received);
        }

        public void Receive(byte[] packet)
        {
            ReceivedPacket received = ackSystem.Receive(packet);
            if (!received.isValid) { return; }

            CheckSentQueue(received);
        }

        private void CheckSentQueue(ReceivedPacket received)
        {
            while (sent.Count > 0)
            {
                NotifyToken token = sent.Peek();

                int distance = (int)ackSystem.sequencer.Distance(received.receivedSequence, token.Sequence);

                // negative distance means next is sent after last ack, so nothing to ack yet
                if (distance < 0)
                    return;

                // positive distance means it should have been acked, or mark it as lost
                sent.Dequeue();

                // if distance above size then it is outside of mask, so set as lost
                bool lost = OutsideOfMask(distance) || NotInMask(distance, received.receivedMask);

                token.Notify(!lost);
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

        public void Update()
        {
            ackSystem.Update();
        }
    }

    internal class ReliableOrderSystem
    {
        // todo https://gafferongames.com/post/reliable_ordered_messages/

        readonly AckSystem ackSystem;
        const int MaxSentQueue = 512;
        readonly Queue<SequencePacket> sent = new Queue<SequencePacket>(MaxSentQueue);

        public ReliableOrderSystem(AckSystem ackSystem)
        {
            this.ackSystem = ackSystem;
        }

        public void Send(byte[] packet)
        {
            if (sent.Count >= MaxSentQueue)
            {
                throw new InvalidOperationException("Sent queue is full");
            }

            // todo add sequence to packet
            //   onRecieve only send packet to higher level if in order

            ushort sequence = ackSystem.Send(packet);
            sent.Enqueue(new SequencePacket(sequence, packet));
        }

        public void Receive(byte[] packet)
        {
            ReceivedPacket received = ackSystem.Receive(packet);
            if (!received.isValid) { return; }

            CheckSentQueue(received);
        }

        private void CheckSentQueue(ReceivedPacket received)
        {
            while (sent.Count > 0)
            {
                SequencePacket packet = sent.Peek();

                int distance = (int)ackSystem.sequencer.Distance(received.receivedSequence, packet.sequence);

                // negative distance means next is sent after last ack, so nothing to ack yet
                if (distance < 0)
                    return;

                // positive distance means it should have been acked, or mark it as lost
                sent.Dequeue();

                // if distance above size then it is outside of mask, so set as lost
                bool lost = OutsideOfMask(distance) || NotInMask(distance, received.receivedMask);

                Resend(packet);
            }
        }

        private void Resend(SequencePacket packet)
        {
            throw new NotImplementedException();
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

        public struct SequencePacket
        {
            public uint sequence;
            public byte[] packet;

            public SequencePacket(uint sequence, byte[] packet)
            {
                this.sequence = sequence;
                this.packet = packet;
            }
        }
    }
    public class RingBuffer
    {
        SequencePacket[] ring;
        ulong head;
        ulong tail;
        Sequencer sequencer;

        public RingBuffer()
        {
            head = 0;
            tail = 0;
            sequencer = new Sequencer(256);
            ring = new SequencePacket[8];
        }


        public bool IsFull()
        {
            long distance = sequencer.Distance(head, tail);

            return distance == -1;
        }

        public void AddNext(uint sequence, byte[] packet)
        {
            ring[head] = new SequencePacket(sequence, packet);
            head = sequencer.NextAfter(head);
        }
        public void Remove(uint sequence)
        {
            throw new NotImplementedException();
        }

        public struct SequencePacket
        {
            public uint sequence;
            public byte[] packet;

            public SequencePacket(uint sequence, byte[] packet)
            {
                this.sequence = sequence;
                this.packet = packet;
            }
        }
    }
}
