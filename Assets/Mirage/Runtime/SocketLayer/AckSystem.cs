using System;
using UnityEngine.Assertions;

namespace Mirage.SocketLayer
{
    struct ReceivedPacket
    {
        public ushort receivedSequence;
        public uint receivedMask;

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
        public const int HEADER_SIZE_ACK = 7;
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

            SetSendTime();
        }

        void SetSendTime()
        {
            lastSentTime = time.Now;
        }

        public ReceivedPacket Receive(byte[] packet)
        {
            int offset = 0;
            var packetType = (PacketType)ByteUtils.ReadByte(packet, ref offset);
            // todo replace assert with log
            Assert.AreEqual(packetType, PacketType.Notify);

            ushort sequence = ByteUtils.ReadUShort(packet, ref offset);
            ushort receivedSequence = ByteUtils.ReadUShort(packet, ref offset);
            uint receivedMask = ByteUtils.ReadUInt(packet, ref offset);

            // todo replace assert with log
            Assert.AreEqual(offset, HEADER_SIZE);

            if (SetReievedNumbers(sequence))
            {
                return new ReceivedPacket
                {
                    isValid = true,
                    receivedSequence = receivedSequence,
                    receivedMask = receivedMask,
                };
            }
            else
            {
                return ReceivedPacket.Invalid();
            }
        }

        public ReceivedPacket ReceiveAck(byte[] packet)
        {
            int offset = 0;
            var packetType = (PacketType)ByteUtils.ReadByte(packet, ref offset);
            // todo replace assert with log
            Assert.AreEqual(packetType, PacketType.NotifyAck);

            ushort receivedSequence = ByteUtils.ReadUShort(packet, ref offset);
            uint receivedMask = ByteUtils.ReadUInt(packet, ref offset);

            // todo replace assert with log
            Assert.AreEqual(offset, HEADER_SIZE_ACK);

            return new ReceivedPacket
            {
                isValid = true,
                receivedSequence = receivedSequence,
                receivedMask = receivedMask,
            };
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

            // todo replace assert with log
            Assert.AreEqual(offset, HEADER_SIZE);

            connection.SendRaw(final, final.Length);
            SetSendTime();

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
            // todo use pool to stop allocations
            byte[] final = new byte[HEADER_SIZE_ACK];
            int offset = 0;

            ByteUtils.WriteByte(final, ref offset, (byte)PacketType.NotifyAck);

            ByteUtils.WriteUShort(final, ref offset, receivedSequence);
            ByteUtils.WriteUInt(final, ref offset, receivedMask);

            // todo replace assert with log
            Assert.AreEqual(offset, HEADER_SIZE_ACK);

            connection.SendRaw(final, final.Length);
            SetSendTime();
        }
    }
}
