using System;
using UnityEngine;

namespace Mirage.SocketLayer
{
    /// <summary>
    /// Objects that represends a connection to/from a server/client. Holds state that is needed to update, send, and receive data
    /// </summary>
    internal sealed class ReliableConnection : Connection, IRawConnection
    {
        private readonly AckSystem _ackSystem;
        private readonly Pool<ByteBuffer> _bufferPool;

        internal ReliableConnection(Peer peer, IEndPoint endPoint, IDataHandler dataHandler, Config config, int maxPacketSize, Time time, Pool<ByteBuffer> bufferPool, ILogger logger, Metrics metrics)
            : base(peer, endPoint, dataHandler, config, maxPacketSize, time, logger, metrics)
        {

            _bufferPool = bufferPool;
            _ackSystem = new AckSystem(this, config, maxPacketSize, time, bufferPool, metrics);
        }


        void IRawConnection.SendRaw(byte[] packet, int length)
        {
            _peer.Send(this, packet, length);
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

        /// <summary>
        /// single message, batched by AckSystem
        /// </summary>
        /// <param name="message"></param>
        public override void SendReliable(byte[] message, int offset, int length)
        {
            ThrowIfNotConnectedOrConnecting();
            _ackSystem.SendReliable(message, offset, length);
            _metrics?.OnSendMessageReliable(length);
        }

        public override void SendUnreliable(byte[] packet, int offset, int length)
        {
            ThrowIfNotConnectedOrConnecting();

            if (length + 1 > _maxPacketSize)
            {
                throw new ArgumentException($"Message is bigger than MTU, size:{length} but max Unreliable message size is {_maxPacketSize - 1}");
            }

            using (var buffer = _bufferPool.Take())
            {
                Buffer.BlockCopy(packet, offset, buffer.array, 1, length);
                // set header
                buffer.array[0] = (byte)PacketType.Unreliable;

                _peer.Send(this, buffer.array, length + 1);
            }

            _metrics?.OnSendMessageUnreliable(length);
        }

        internal override void ReceiveUnreliablePacket(Packet packet)
        {
            var count = packet.Length - 1;
            var segment = new ArraySegment<byte>(packet.Buffer.array, 1, count);
            _dataHandler.ReceiveMessage(this, segment);


            _metrics?.OnReceiveMessageUnreliable(count);
        }

        internal override void ReceiveReliablePacket(Packet packet)
        {
            _ackSystem.ReceiveReliable(packet.Buffer.array, packet.Length, false);

            HandleQueuedMessages();
        }

        internal override void ReceiveReliableFragment(Packet packet)
        {
            if (_ackSystem.InvalidFragment(packet.Buffer.array))
            {
                Disconnect(DisconnectReason.InvalidPacket);
                return;
            }

            _ackSystem.ReceiveReliable(packet.Buffer.array, packet.Length, true);

            HandleQueuedMessages();
        }

        private void HandleQueuedMessages()
        {
            // gets messages in order
            while (_ackSystem.NextReliablePacket(out var received))
            {
                if (received.IsFragment)
                {
                    HandleFragmentedMessage(received);
                }
                else
                {
                    HandleBatchedMessageInPacket(received);
                }
            }
        }

        private void HandleFragmentedMessage(AckSystem.ReliableReceived received)
        {
            // get index from first
            var firstArray = received.Buffer.array;
            // length +1 because zero indexed 
            var fragmentLength = firstArray[0] + 1;

            // todo find way to remove allocation? (can't use buffers because they will be too small for this bigger message)
            var message = new byte[fragmentLength * _ackSystem.SizePerFragment];

            // copy first
            var copyLength = received.Length - 1;
            _logger?.Assert(copyLength == _ackSystem.SizePerFragment, "First should be max size");
            Buffer.BlockCopy(firstArray, 1, message, 0, copyLength);
            received.Buffer.Release();

            var messageLength = copyLength;
            // start at 1 because first copied above
            for (var i = 1; i < fragmentLength; i++)
            {
                var next = _ackSystem.GetNextFragment();
                var nextArray = next.Buffer.array;

                _logger?.Assert(i == (fragmentLength - 1 - nextArray[0]), "fragment index should decrement each time");

                // +1 because first is copied above
                copyLength = next.Length - 1;
                Buffer.BlockCopy(nextArray, 1, message, _ackSystem.SizePerFragment * i, copyLength);
                messageLength += copyLength;
                next.Buffer.Release();
            }

            _metrics?.OnReceiveMessageReliable(messageLength);
            _dataHandler.ReceiveMessage(this, new ArraySegment<byte>(message, 0, messageLength));
        }

        private void HandleBatchedMessageInPacket(AckSystem.ReliableReceived received)
        {
            var array = received.Buffer.array;
            var packetLength = received.Length;
            var offset = 0;
            HandleReliableBatched(array, offset, packetLength);

            // release buffer after all its message have been handled
            received.Buffer.Release();
        }

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

                case PacketType.Unreliable:
                    return length >= minUnreliableSize;

                case PacketType.Notify:
                    return length >= AckSystem.NOTIFY_HEADER_SIZE + minMessageSize;
                case PacketType.Reliable:
                    return length >= AckSystem.MIN_RELIABLE_HEADER_SIZE + minMessageSize;
                case PacketType.ReliableFragment:
                    return length >= AckSystem.MIN_RELIABLE_FRAGMENT_HEADER_SIZE + 1;
                case PacketType.Ack:
                    return length >= AckSystem.ACK_HEADER_SIZE;

                default:
                case PacketType.KeepAlive:
                    return true;
            }
        }
    }
}
