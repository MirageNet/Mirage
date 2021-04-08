using System;

namespace Mirage.SocketLayer
{
    struct ReceivedPacket
    {
        public ulong sequence;
        public ulong receivedSequence;
        public ulong receivedMask;

        public ArraySegment<byte> packet;
    }

    internal class AckSystem
    {
        const int HEADER_SIZE = 9;
        Sequencer sequencer = new Sequencer(16);

        ulong receivedSequence;
        ulong receivedMask;
        readonly IRawConnection connection;

        public AckSystem(IRawConnection connection)
        {
            this.connection = connection;
        }

        public ReceivedPacket Receive(byte[] packet)
        {
            // todo assert packet is notify

            ulong sequence = (ulong)(packet[1] | (packet[2] << 8));
            ulong receivedSequence = (ulong)(packet[3] | (packet[4] << 8));
            ulong receivedMask = (ulong)(packet[5] | (packet[6] << 8) | (packet[7] << 16) | (packet[8] << 24));

            setReievedNumbers(sequence);

            return new ReceivedPacket
            {
                sequence = sequence,
                receivedSequence = receivedSequence,
                receivedMask = receivedMask,
                packet = new ArraySegment<byte>(packet, HEADER_SIZE, packet.Length - HEADER_SIZE)
            };
        }

        private void setReievedNumbers(ulong sequence)
        {
            // todo check that new number is after current
            receivedSequence = sequence;
            // todo create mask based on new sequence vs old seqence, eg if old is -1 then just left shift by 1 and add 1
            receivedMask = (receivedMask << 1) | 1UL;
        }

        // todo keep track of sequence and packet nunber
        public void Send(byte[] packet)
        {
            // todo check packet size is within MTU

            byte[] final = new byte[packet.Length + HEADER_SIZE];
            Buffer.BlockCopy(packet, 0, final, HEADER_SIZE, packet.Length);

            // todo use bitwriter
            // todo merge Reliable and notify?
            final[0] = (byte)PacketType.Notify;

            // number for this packet
            ulong sequence = sequencer.Next();
            final[1] = (byte)sequence;
            final[2] = (byte)(sequence >> 8);

            // ack
            // number for most recent received packet
            final[3] = (byte)receivedSequence;
            final[4] = (byte)(receivedSequence >> 8);

            // bit mask for older packets 
            final[5] = (byte)receivedSequence;
            final[6] = (byte)(receivedSequence >> 8);
            final[7] = (byte)(receivedSequence >> 16);
            final[8] = (byte)(receivedSequence >> 24);

            connection.SendRaw(final);
        }
    }
}
