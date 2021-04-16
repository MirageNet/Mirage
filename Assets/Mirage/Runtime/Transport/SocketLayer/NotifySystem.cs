using System;
using System.Collections.Generic;

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
        internal ushort Sequence;

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
        readonly float timeout;


        const int MaxSentQueue = 512;
        Queue<NotifyToken> sent = new Queue<NotifyToken>(MaxSentQueue);

        public NotifySystem(IRawConnection connection, float timeout, float sendAckTime, Time time)
        {
            ackSystem = new AckSystem(connection, sendAckTime, time);
            this.timeout = timeout;
        }

        public NotifyToken Send(byte[] packet)
        {
            if (sent.Count >= MaxSentQueue)
            {
                throw new InvalidOperationException("Sent queue is full");
            }

            // todo get sequence from ack system and assign to token
            ackSystem.Send(packet);

            // todo add token to queue
            // todo use pool to stop allocations
            return new NotifyToken();
        }
        public void Receive(byte[] packet)
        {
            // tdo check order of packet, do we drop old packets?
            // todo get messages from pecket and invoke
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

                const int maskSize = sizeof(uint) * 8;
                // if distance above size then it is outside of mask, so set as lost

                bool outsideOfMask = distance > maskSize;

                uint ackBit = 1u << distance;
                bool notInMask = (received.receivedMask & ackBit) == 0u;

                // todo clean this code up with multiple methods
                bool lost = outsideOfMask || notInMask;

                token.Notify(!lost);
            }
        }

        public void Update()
        {
            ackSystem.Update();
        }
    }
}
