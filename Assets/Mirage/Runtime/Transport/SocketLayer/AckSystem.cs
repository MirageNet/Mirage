using System;
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace Mirage.SocketLayer
{
    internal static class ByteUtils
    {
        public static void WriteByte(byte[] buffer, ref int offset, byte value)
        {
            buffer[offset] = value;
            offset++;
        }

        public static byte ReadByte(byte[] buffer, ref int offset)
        {
            byte a = buffer[offset];
            offset++;

            return a;
        }


        public static void WriteUShort(byte[] buffer, ref int offset, ushort value)
        {
            buffer[offset] = (byte)value;
            offset++;
            buffer[offset] = (byte)(value >> 8);
            offset++;
        }

        public static ushort ReadUShort(byte[] buffer, ref int offset)
        {
            ushort a = buffer[offset];
            offset++;
            ushort b = buffer[offset];
            offset++;

            return (ushort)(a | (b << 8));
        }


        public static void WriteUInt(byte[] buffer, ref int offset, uint value)
        {
            buffer[offset] = (byte)value;
            offset++;
            buffer[offset] = (byte)(value >> 8);
            offset++;
            buffer[offset] = (byte)(value >> 16);
            offset++;
            buffer[offset] = (byte)(value >> 24);
            offset++;
        }

        public static uint ReadUInt(byte[] buffer, ref int offset)
        {
            uint a = buffer[offset];
            offset++;
            uint b = buffer[offset];
            offset++;
            uint c = buffer[offset];
            offset++;
            uint d = buffer[offset];
            offset++;

            return a | (b << 8) | (c << 16) | (d << 24);
        }
    }

    struct ReceivedPacket
    {
        public ushort sequence;
        public ushort receivedSequence;
        public uint receivedMask;

        public ArraySegment<byte> packet;
        /// <summary>
        /// should packet be used and sent higher up to mirage
        /// <para>It could be invalid because it as a duplicate packet or arrived late</para>
        /// </summary>
        public bool isValid;

        public static ReceivedPacket Invalid() => new ReceivedPacket { isValid = false };
    }

    internal class AckSystem
    {
        const int HEADER_SIZE = 9;
        public readonly Sequencer sequencer = new Sequencer(16);

        ushort receivedSequence;
        uint receivedMask;
        bool[] received = new bool[sizeof(int) * 8];
        bool[] receivedNext = new bool[sizeof(int) * 8];
        readonly IRawConnection connection;
        readonly Time time;
        float lastSentTime;

        public AckSystem(IRawConnection connection, Time time)
        {
            this.connection = connection;
            this.time = time;

            // set received to first sequence
            // this means that it will always be 1 before first sent packet
            // so first receieved will have correcct distance
            receivedSequence = (ushort)sequencer.Next();
        }

        public ReceivedPacket Receive(byte[] packet)
        {
            // todo assert packet is notify
            int offset = 0;
            var packetType = (PacketType)ByteUtils.ReadByte(packet, ref offset);
            Assert.AreEqual(packetType, PacketType.Notify);


            ushort sequence = ByteUtils.ReadUShort(packet, ref offset);
            ushort receivedSequence = ByteUtils.ReadUShort(packet, ref offset);
            uint receivedMask = ByteUtils.ReadUInt(packet, ref offset);

            Assert.AreEqual(offset, HEADER_SIZE);

            if (setReievedNumbers(sequence))
            {
                return new ReceivedPacket
                {
                    isValid = true,
                    sequence = sequence,
                    receivedSequence = receivedSequence,
                    receivedMask = receivedMask,
                    packet = new ArraySegment<byte>(packet, HEADER_SIZE, packet.Length - HEADER_SIZE)
                };
            }
            else
            {
                return ReceivedPacket.Invalid();
            }
        }

        private bool setReievedNumbers(ushort sequence)
        {
            // todo optimzie this
            long distance = sequencer.Distance(receivedSequence, sequence);

            // duplicate
            if (distance == 0) { return false; }
            // arrived late
            if (distance > 1) { return false; }

            // if no dropped distance should be -1

            // calculate next receive mask
            for (int i = 0; i < received.Length; i++)
            {
                // if to stop out of range
                long next = i - distance;
                if (next >= 0 && next < received.Length)
                    receivedNext[next] = received[i];
            }
            // first is current packet so should be true
            receivedNext[0] = true;

            // copy to received
            uint mask = 0;
            for (int i = 0; i < received.Length; i++)
            {
                received[i] = receivedNext[i];

                // set bit if received
                mask |= received[i] ? 1u : 0u;
                // shift
                mask <<= 1;
            }

            //create mask in reverce order
            for (int i = received.Length - 1; i >= 0; i--)
            {
                // set bit if received
                mask |= received[i] ? 1u : 0u;

                // shift (not not last)
                if (i != 0)
                {
                    mask <<= 1;
                }
            }

            receivedSequence = sequence;
            receivedMask = mask;

            return true;
        }

        // todo keep track of sequence and packet nunber
        public void Send(byte[] packet)
        {
            // todo check packet size is within MTU

            byte[] final = new byte[packet.Length + HEADER_SIZE];
            Buffer.BlockCopy(packet, 0, final, HEADER_SIZE, packet.Length);

            int offset = 0;

            ByteUtils.WriteByte(final, ref offset, (byte)PacketType.Notify);

            ulong sequence = sequencer.Next();
            ByteUtils.WriteUShort(final, ref offset, (ushort)sequence);
            ByteUtils.WriteUShort(final, ref offset, receivedSequence);
            ByteUtils.WriteUInt(final, ref offset, receivedMask);

            Assert.AreEqual(offset, HEADER_SIZE);

            connection.SendRaw(final);
            lastSentTime = time.Now;
        }


        public void Update()
        {
            // todo send ack if not recently been sent
            // ack only packet sent if no other sent within last frame
        }
    }

    internal class NotifySystem
    {
        readonly AckSystem ackSystem;
        readonly float timeout;


        const int MaxSentQueue = 512;
        Queue<NotifyToken> sent = new Queue<NotifyToken>(MaxSentQueue);

        public NotifySystem(IRawConnection connection, float timeout, Time time)
        {
            ackSystem = new AckSystem(connection, time);
            this.timeout = timeout;
        }

        public NotifyToken SendMessage(byte[] packet)
        {
            if (sent.Count >= MaxSentQueue)
            {
                throw new InvalidOperationException("Sent queue is full");
            }

            ackSystem.Send(packet);
            return new NotifyToken();
        }
        public void Receive(byte[] packet)
        {
            //todo get messages from pecket and invoke
            ReceivedPacket received = ackSystem.Receive(packet);
            while (sent.Count > 0)
            {
                NotifyToken next = sent.Peek();

                int distance = (int)ackSystem.sequencer.Distance(next.Sequence, received.receivedSequence);

                // posititve distance means next is sent after last ack, so nothing to ack yet
                if (distance > 0)
                    return;

                // negative distance means it should have been acked, or mark it as lost
                sent.Dequeue();

                const int maskSize = sizeof(uint) * 8;
                int posDistance = -distance;
                // if distance above size then it is outside of mask, so set as lost

                bool outsideOfMask = posDistance > maskSize;

                uint ackBit = 1u << posDistance;
                bool notInMask = (received.receivedMask & ackBit) == 0u;

                // todo clean this code up with multiple methods
                bool lost = outsideOfMask || notInMask;

                next.Notify(!lost);
            }
        }

        public void Update()
        {
            ackSystem.Update();
        }
    }

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

    public class ReliableSystem
    {
        NotifySystem notifySystem;

        Queue<Sent> sent;

        public void Send(byte[] packet)
        {
            // todo implement
            //   send packet using notify
            //   if lost resend
            //   if received twice, ignore 2nd
            throw new NotImplementedException();
        }

        struct Sent
        {
            byte[] packet;
            NotifyToken token;
        }
    }
}
