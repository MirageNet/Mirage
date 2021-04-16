using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Mirage.SocketLayer
{
    /// <summary>
    /// Object returned from <see cref="NotifySystem.Send(byte[])"/> with events for when packet is Lost or Delivered
    /// </summary>
    public class NotifyToken
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
            ackSystem = new AckSystem(connection, ackTimeout, time);
        }

        public NotifyToken Send(byte[] packet)
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
        public void Receive(byte[] packet)
        {
            ReceivedPacket received = ackSystem.Receive(packet);
            if (!received.isValid) { return; }

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
}
