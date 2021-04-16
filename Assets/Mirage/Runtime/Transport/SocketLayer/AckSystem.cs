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
        readonly IRawConnection connection;
        readonly Time time;
        readonly float ackTimeout;
        float lastSentTime;


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

            if (SetReievedNumbers(sequence))
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

        private bool SetReievedNumbers(ushort sequence)
        {
            int distance = (int)sequencer.Distance(sequence, receivedSequence);

            // duplicate or arrived late
            if (distance <= 0) { return false; }

            // shift mask by distance, then add 1
            // eg distance = 2
            // this will mean mask will be ..01
            // which means that 1 packet was missed
            receivedMask = (receivedMask << distance) | 1;

            receivedSequence = sequence;

            return true;
        }

        /// <summary>
        /// </summary>
        /// <param name="packet"></param>
        /// <returns>sequence number of sent packet</returns>
        public ushort Send(byte[] packet)
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

            return (ushort)sequence;
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
            // todo Send ack without a packet
        }
    }
}
