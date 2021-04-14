using System;
using UnityEngine.Assertions;

namespace Mirage.SocketLayer
{
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
        public const int HEADER_SIZE = 9;
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
            // todo use pool to stop allocations
            byte[] final = new byte[packet.Length + HEADER_SIZE];
            Buffer.BlockCopy(packet, 0, final, HEADER_SIZE, packet.Length);

            int offset = 0;

            ByteUtils.WriteByte(final, ref offset, (byte)PacketType.Notify);

            ulong sequence = sequencer.Next();
            ByteUtils.WriteUShort(final, ref offset, (ushort)sequence);
            ByteUtils.WriteUShort(final, ref offset, receivedSequence);
            ByteUtils.WriteUInt(final, ref offset, receivedMask);

            Assert.AreEqual(offset, HEADER_SIZE);

            connection.SendRaw(final, final.Length);
            lastSentTime = time.Now;
        }


        public void Update()
        {
            // todo send ack if not recently been sent
            // ack only packet sent if no other sent within last frame
        }
    }

}
