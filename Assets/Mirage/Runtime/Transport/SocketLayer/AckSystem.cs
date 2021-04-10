using System;
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
        public ulong sequence;
        public ulong receivedSequence;
        public ulong receivedMask;

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
        Sequencer sequencer = new Sequencer(16);

        // start at 
        ushort receivedSequence;
        uint receivedMask;
        bool[] received = new bool[sizeof(int) * 8];
        bool[] receivedNext = new bool[sizeof(int) * 8];
        readonly IRawConnection connection;

        public AckSystem(IRawConnection connection)
        {
            this.connection = connection;

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
        }
    }
}
