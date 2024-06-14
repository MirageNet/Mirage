using System;
using UnityEngine;

namespace Mirage.SocketLayer
{
    internal class PassthroughConnection : Connection, IRawConnection
    {
        private const int HEADER_SIZE = 1 + Batch.MESSAGE_LENGTH_SIZE;

        private readonly Batch _reliableBatch;
        private readonly Batch _unreliableBatch;
        private readonly AckSystem _ackSystem;

        public PassthroughConnection(Peer peer, IEndPoint endPoint, IDataHandler dataHandler, Config config, SocketInfo socketInfo, Time time, Pool<ByteBuffer> bufferPool, ILogger logger, Metrics metrics)
            : base(peer, endPoint, dataHandler, config, socketInfo, time, logger, metrics)
        {
            _reliableBatch = new ArrayBatch(socketInfo.MaxReliableSize, logger, this, PacketType.Reliable, SendMode.Reliable);
            _unreliableBatch = new ArrayBatch(socketInfo.MaxUnreliableSize, logger, this, PacketType.Unreliable, SendMode.Unreliable);
            _ackSystem = new AckSystem(this, config, socketInfo.MaxUnreliableSize, time, bufferPool, logger, metrics);
        }

        /// <summary>
        /// single message, batched by AckSystem
        /// </summary>
        /// <param name="message"></param>
        public override void SendReliable(byte[] message, int offset, int length)
        {
            ThrowIfNotConnectedOrConnecting();

            if (length + HEADER_SIZE > _socketInfo.MaxReliableSize)
            {
                throw new ArgumentException($"Message is bigger than MTU, size:{length} but max message size is {_socketInfo.MaxReliableSize - HEADER_SIZE}");
            }

            _reliableBatch.AddMessage(message, offset, length);
            _metrics?.OnSendMessageReliable(length);
        }

        public override void SendUnreliable(byte[] message, int offset, int length)
        {
            ThrowIfNotConnectedOrConnecting();

            if (length + HEADER_SIZE > _socketInfo.MaxUnreliableSize)
            {
                throw new ArgumentException($"Message is bigger than MTU, size:{length} but max message size is {_socketInfo.MaxUnreliableSize - HEADER_SIZE}");
            }

            _unreliableBatch.AddMessage(message, offset, length);
            _metrics?.OnSendMessageUnreliable(length);
        }

        /// <summary>
        /// Use <see cref="INotifyCallBack"/> version for non-alloc
        /// </summary>
        public override INotifyToken SendNotify(byte[] packet, int offset, int length)
        {
            ThrowIfNotConnectedOrConnecting();
            var token = _ackSystem.SendNotify(packet, offset, length);
            _metrics?.OnSendMessageNotify(length);
            return token;
        }

        /// <summary>
        /// Use <see cref="INotifyCallBack"/> version for non-alloc
        /// </summary>
        public override void SendNotify(byte[] packet, int offset, int length, INotifyCallBack callBacks)
        {
            ThrowIfNotConnectedOrConnecting();
            _ackSystem.SendNotify(packet, offset, length, callBacks);
            _metrics?.OnSendMessageNotify(length);
        }

        internal override void ReceiveUnreliablePacket(Packet packet)
        {
            HandleReliableBatched(packet.Buffer.array, 1, packet.Length, PacketType.Unreliable);
        }

        internal override void ReceiveReliablePacket(Packet packet)
        {
            HandleReliableBatched(packet.Buffer.array, 1, packet.Length, PacketType.Reliable);
        }

        internal override void ReceiveReliableFragment(Packet packet) => throw new NotSupportedException();

        internal override void ReceiveNotifyPacket(Packet packet)
        {
            var segment = _ackSystem.ReceiveNotify(packet.Buffer.array, packet.Length);
            if (segment != default)
            {
                _metrics?.OnReceiveMessageNotify(packet.Length);
                _dataHandler.ReceiveMessage(this, segment);
            }
        }

        internal override void ReceiveNotifyAck(Packet packet)
        {
            _ackSystem.ReceiveAck(packet.Buffer.array);
        }

        public override void FlushBatch()
        {
            _ackSystem.Update();
            _reliableBatch.Flush();
            _unreliableBatch.Flush();
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
            const int minUnreliableSize = 1 + minMessageSize;

            switch (packet.Type)
            {
                case PacketType.Command:
                    return length >= minCommandSize;

                case PacketType.Reliable:
                case PacketType.Unreliable:
                    return length >= minUnreliableSize;

                case PacketType.Notify:
                    return length >= AckSystem.NOTIFY_HEADER_SIZE + minMessageSize;
                case PacketType.Ack:
                    return length >= AckSystem.ACK_HEADER_SIZE;
                case PacketType.ReliableFragment:
                    // not supported
                    return false;

                default:
                case PacketType.KeepAlive:
                    return true;
            }
        }
    }
}
