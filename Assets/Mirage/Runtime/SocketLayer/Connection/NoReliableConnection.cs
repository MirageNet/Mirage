using System;
using UnityEngine;

namespace Mirage.SocketLayer
{
    /// <summary>
    /// Connection that does not run its own reliablity layer, good for TCP sockets
    /// </summary>
    internal sealed class NoReliableConnection : Connection
    {
        private const int HEADER_SIZE = 1 + MESSAGE_LENGTH_SIZE;
        private const int MESSAGE_LENGTH_SIZE = 2;

        private byte[] _nextBatch;
        private int _batchLength;

        internal NoReliableConnection(Peer peer, IEndPoint endPoint, IDataHandler dataHandler, Config config, int maxPacketSize, Time time, ILogger logger, Metrics metrics)
            : base(peer, endPoint, dataHandler, config, maxPacketSize, time, logger, metrics)
        {
            _nextBatch = new byte[maxPacketSize];
            CreateNewBatch();

            if (maxPacketSize > ushort.MaxValue)
            {
                throw new ArgumentException($"Max package size can not bigger than {ushort.MaxValue}. NoReliableConnection uses 2 bytes for message length, maxPacketSize over that value will mean that message will be incorrectly batched.");
            }
        }


        public override void SendUnreliable(byte[] packet, int offset, int length) => SendReliable(packet, offset, length);
        public override void SendNotify(byte[] packet, int offset, int length, INotifyCallBack callBacks)
        {
            SendReliable(packet, offset, length);
            callBacks.OnDelivered();
        }

        public override INotifyToken SendNotify(byte[] packet, int offset, int length)
        {
            SendReliable(packet, offset, length);
            return AutoCompleteToken.Instance;
        }

        /// <summary>
        /// single message, batched by AckSystem
        /// </summary>
        /// <param name="message"></param>
        public override void SendReliable(byte[] message, int offset, int length)
        {
            ThrowIfNotConnectedOrConnecting();

            if (length + HEADER_SIZE > _maxPacketSize)
            {
                throw new ArgumentException($"Message is bigger than MTU, size:{length} but max message size is {_maxPacketSize - HEADER_SIZE}");
            }


            var msgLength = length + MESSAGE_LENGTH_SIZE;
            if (_batchLength + msgLength > _maxPacketSize)
            {
                // if full, send and create new
                SendBatch();
            }

            AddToBatch(message, offset, length);
            _metrics?.OnSendMessageReliable(length);
        }

        private void SendBatch()
        {
            _peer.Send(this, _nextBatch, _batchLength);
            CreateNewBatch();
        }

        private void CreateNewBatch()
        {
            _nextBatch[0] = (byte)PacketType.Reliable;
            _batchLength = 1;
        }

        private void AddToBatch(byte[] message, int offset, int length)
        {
            ByteUtils.WriteUShort(_nextBatch, ref _batchLength, checked((ushort)length));
            Buffer.BlockCopy(message, offset, _nextBatch, _batchLength, length);
            _batchLength += length;
        }

        internal override void ReceiveReliablePacket(Packet packet)
        {
            HandleReliableBatched(packet.Buffer.array, 1, packet.Length);
        }

        internal override void ReceiveUnreliablePacket(Packet packet) => throw new NotSupportedException();
        internal override void ReceiveReliableFragment(Packet packet) => throw new NotSupportedException();
        internal override void ReceiveNotifyPacket(Packet packet) => throw new NotSupportedException();
        internal override void ReceiveNotifyAck(Packet packet) => throw new NotSupportedException();

        public override void FlushBatch()
        {
            if (_batchLength > 1)
            {
                SendBatch();
            }
        }

        internal override bool IsValidSize(Packet packet)
        {
            const int minPacketSize = 1;

            var length = packet.Length;
            if (length < minPacketSize)
                return false;

            // Min size of message given to Mirage
            const int minMessageSize = 2;

            const int minCommandSize = 2;
            // 1 msgType + 2 msgSize + 2 MirageMsg
            const int minSize = 1 + 2 + minMessageSize;

            switch (packet.Type)
            {
                case PacketType.Command:
                    return length >= minCommandSize;

                case PacketType.Reliable:
                    return length >= minSize;

                case PacketType.Ack:
                case PacketType.ReliableFragment:
                case PacketType.Notify:
                case PacketType.Unreliable:
                    // none of these are expected when using NoReliableConnetion
                    return false;

                default:
                case PacketType.KeepAlive:
                    return true;
            }
        }
    }
}
