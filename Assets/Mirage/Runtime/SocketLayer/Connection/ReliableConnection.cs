using System;
using UnityEngine;

namespace Mirage.SocketLayer
{
    /// <summary>
    /// Objects that represents a connection to/from a server/client. Holds state that is needed to update, send, and receive data
    /// </summary>
    internal sealed class ReliableConnection : Connection, IRawConnection, IDisposable
    {
        private readonly AckSystem _ackSystem;
        private readonly Batch _unreliableBatch;
        private readonly Pool<ByteBuffer> _bufferPool;

        internal RingBuffer<AckSystem.AckablePacket> AckablePackets => _ackSystem.SentAckablePackets;
        internal RingBuffer<AckSystem.ReliableReceived> ReliableReceive => _ackSystem.ReliableReceive;

        internal ReliableConnection(Peer peer, IConnectionHandle handle, IDataHandler dataHandler, Config config, int maxPacketSize, Time time, Pool<ByteBuffer> bufferPool,
            Pool<AckSystem.ReliablePacket> reliablePool, RingBuffer<AckSystem.AckablePacket> ackablePacket, RingBuffer<AckSystem.ReliableReceived> reliableReceive,
            ILogger logger, Metrics metrics)
            : base(peer, handle, dataHandler, config, maxPacketSize, time, logger, metrics)
        {
            _bufferPool = bufferPool;
            _unreliableBatch = new ArrayBatch(_maxPacketSize, SendBatchInternal, PacketType.Unreliable);
            _ackSystem = new AckSystem(this, config, maxPacketSize, time, bufferPool,
                reliablePool, ackablePacket, reliableReceive,
                () => DisconnectInternal(DisconnectReason.InvalidPacket), logger, metrics);
        }

        private void SendBatchInternal(byte[] batch, int length)
        {
            _peer.Send(this, batch, length);
        }

        public void Dispose()
        {
            _ackSystem.Dispose();
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

            const int batchHeader = 1 + Batch.MESSAGE_LENGTH_SIZE;
            if (length + batchHeader > _maxPacketSize)
            {
                throw new MessageSizeException($"Message is bigger than MTU, size:{length} but max Unreliable message size is {_maxPacketSize - batchHeader}");
            }

            _unreliableBatch.AddMessage(packet, offset, length);
            _metrics?.OnSendMessageUnreliable(length);
        }

        internal override void ReceiveUnreliablePacket(Packet packet)
        {
            HandleReliableBatched(packet.Span[1..], PacketType.Unreliable);
        }

        internal override void ReceiveReliablePacket(Packet packet)
        {
            _ackSystem.ReceiveReliable(packet.Span, false);

            HandleQueuedMessages();
        }

        internal override void ReceiveReliableFragment(Packet packet)
        {
            if (_ackSystem.InvalidFragment(packet.Span))
            {
                if (_logger.Enabled(LogType.Error))
                {
                    if (_peer._fragmentBuffer == null)
                        _logger.Log(LogType.Error, "Received fragmented message but fragmentation is disabled (MaxReliableFragments is 0 or less).");
                    else
                        _logger.Log(LogType.Error, "Received invalid fragment. Disconnecting.");
                }

                DisconnectInternal(DisconnectReason.InvalidPacket);
                return;
            }

            _ackSystem.ReceiveReliable(packet.Span, true);

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
            var requiredSize = fragmentLength * _ackSystem.SizePerFragment;

            // AckSystem.InvalidFragment should have already caught this, 
            // but we check again here as a safety measure before using the shared buffer.
            if (requiredSize > _peer._fragmentBuffer?.Length)
            {
                if (_logger.Enabled(LogType.Error))
                    _logger.Log(LogType.Error, $"Fragment buffer size ({_peer._fragmentBuffer?.Length ?? 0}) is less than required size ({requiredSize}). This should have been caught by InvalidFragment check.");

                CleanupAndDisconnect(received, nextIndex: 1);
                return;
            }

            // If fragmentation is disabled, we shouldn't have received this message
            // Note: we have checks for this earlier, but also check here to be safe
            if (_peer._fragmentBuffer == null)
            {
                if (_logger.Enabled(LogType.Error))
                    _logger.Log(LogType.Error, "Received fragmented message but fragmentation is disabled (MaxReliableFragments is 0 or less).");
                CleanupAndDisconnect(received, nextIndex: 1);
                return;
            }

            byte[] message;
            bool usedShared;
            // Use shared buffer, 
            // if it is in use (it shouldn't be), then fallback to creating new buffer
            // Note: since this is single threaded and `_dataHandler.ReceiveMessage` should NOT hold onto array, 
            //       we can just reuse the same buffer without issue
            if (_peer._fragmentBufferInUse)
            {
                if (_logger.Enabled(LogType.Error))
                    _logger.Log(LogType.Error, "Fragment buffer already in use, falling back to allocation. This should only happen during recursion.");
                message = new byte[requiredSize];
                usedShared = false;
            }
            else
            {
                _peer._fragmentBufferInUse = true;
                usedShared = true;
                message = _peer._fragmentBuffer;
            }

            try
            {
                // copy first
                var copyLength = received.Length - 1;
                if (copyLength > _ackSystem.SizePerFragment)
                {
                    if (_logger.Enabled(LogType.Error)) _logger.Error($"First fragment length ({copyLength}) exceeds SizePerFragment ({_ackSystem.SizePerFragment})");
                    CleanupAndDisconnect(received, nextIndex: 1);
                    return;
                }
                _logger?.Assert(copyLength == _ackSystem.SizePerFragment, "First should be max size");
                Buffer.BlockCopy(firstArray, 1, message, 0, copyLength);
                received.Buffer.Release();

                var messageLength = copyLength;
                // start at 1 because first copied above
                for (var i = 1; i < fragmentLength; i++)
                {
                    var next = _ackSystem.GetNextFragment();
                    var nextArray = next.Buffer.array;

                    // NOTE: only the index of the first fragment is used by code,
                    // so we only need to check the length here in debug builds to catch bugs during testing
                    // Malicious actors can already modify the packet data, changing the fragment index doesn't break anything extra
                    _logger?.Assert(i == (fragmentLength - 1 - nextArray[0]), "fragment index should decrement each time");

                    // +1 because first is copied above
                    copyLength = next.Length - 1;
                    if (copyLength > _ackSystem.SizePerFragment)
                    {
                        if (_logger.Enabled(LogType.Error)) _logger.Error($"Fragment length ({copyLength}) exceeds SizePerFragment ({_ackSystem.SizePerFragment})");
                        CleanupAndDisconnect(next, nextIndex: i + 1);
                        return;
                    }
                    Buffer.BlockCopy(nextArray, 1, message, _ackSystem.SizePerFragment * i, copyLength);
                    messageLength += copyLength;
                    next.Buffer.Release();
                }

                _metrics?.OnReceiveMessageReliable(messageLength);
                _dataHandler.ReceiveMessage(this, new ArraySegment<byte>(message, 0, messageLength));
            }
            finally
            {
                if (usedShared)
                    _peer._fragmentBufferInUse = false;
            }

            // **Local cleanup function**
            void CleanupAndDisconnect(AckSystem.ReliableReceived current, int nextIndex)
            {
                current.Buffer.Release();
                for (var j = nextIndex; j < fragmentLength; j++)
                {
                    var leftover = _ackSystem.GetNextFragment();
                    leftover.Buffer.Release();
                }
                DisconnectInternal(DisconnectReason.InvalidPacket);
            }
        }

        private void HandleBatchedMessageInPacket(AckSystem.ReliableReceived received)
        {
            var array = received.Buffer.array;
            var packetLength = received.Length;
            var offset = 0;
            HandleReliableBatched(array, offset, packetLength, PacketType.Reliable);

            // release buffer after all its message have been handled
            received.Buffer.Release();
        }

        internal override void ReceiveNotifyPacket(Packet packet)
        {
            var span = _ackSystem.ReceiveNotify(packet.Span);
            if (span.Length != 0)
            {
                _metrics?.OnReceiveMessageNotify(packet.Length);
                using (var buffer = _bufferPool.Take())
                {
                    if (span.Length > buffer.array.Length)
                    {
                        _logger.Error("Received a packet that is larger than buffer pool");
                        return;
                    }

                    span.CopyTo(buffer.array);
                    _dataHandler.ReceiveMessage(this, new ArraySegment<byte>(buffer.array, 0, span.Length));
                }
            }
        }

        internal override void ReceiveNotifyAck(Packet packet)
        {
            _ackSystem.ReceiveAck(packet.Span);
        }

        public override void FlushBatch()
        {
            _ackSystem.Update();
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
