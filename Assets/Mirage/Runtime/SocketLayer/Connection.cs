using System;
using UnityEngine;

namespace Mirage.SocketLayer
{
    /// <summary>
    /// Connection for <see cref="Peer"/>
    /// </summary>
    public interface IConnection
    {
        IEndPoint EndPoint { get; }
        ConnectionState State { get; }

        void Disconnect();

        INotifyToken SendNotify(byte[] packet);
        INotifyToken SendNotify(byte[] packet, int offset, int length);
        INotifyToken SendNotify(ArraySegment<byte> packet);

        void SendNotify(byte[] packet, INotifyCallBack callBacks);
        void SendNotify(byte[] packet, int offset, int length, INotifyCallBack callBacks);
        void SendNotify(ArraySegment<byte> packet, INotifyCallBack callBacks);

        /// <summary>
        /// single message, batched by AckSystem
        /// </summary>
        /// <param name="message"></param>
        void SendReliable(byte[] message);
        /// <summary>
        /// single message, batched by AckSystem
        /// </summary>
        /// <param name="message"></param>
        void SendReliable(byte[] message, int offset, int length);
        /// <summary>
        /// single message, batched by AckSystem
        /// </summary>
        /// <param name="message"></param>
        void SendReliable(ArraySegment<byte> message);

        void SendUnreliable(byte[] packet);
        void SendUnreliable(byte[] packet, int offset, int length);
        void SendUnreliable(ArraySegment<byte> packet);

        /// <summary>
        /// Forces the connection to send any batched message immediately to the socket
        /// <para>
        /// Note: this will only send the packet to the socket. Some sockets may not send on main thread so might not send immediately
        /// </para>
        /// </summary>
        void FlushBatch();
    }

    /// <summary>
    /// A connection that can send data directly to sockets
    /// <para>Only things inside socket layer should be sending raw packets. Others should use the methods inside <see cref="Connection"/></para>
    /// </summary>
    internal interface IRawConnection
    {
        /// <summary>
        /// Sends directly to socket without adding header
        /// <para>packet given to this function as assumed to already have a header</para>
        /// </summary>
        /// <param name="packet">header and messages</param>
        void SendRaw(byte[] packet, int length);
    }

    /// <summary>
    /// Objects that represends a connection to/from a server/client. Holds state that is needed to update, send, and receive data
    /// </summary>
    internal sealed class Connection : IConnection, IRawConnection
    {
        private readonly ILogger _logger;

        private readonly Peer _peer;
        public readonly IEndPoint EndPoint;
        private readonly IDataHandler _dataHandler;

        private readonly ConnectingTracker _connectingTracker;
        private readonly TimeoutTracker _timeoutTracker;
        private readonly KeepAliveTracker _keepAliveTracker;
        private readonly DisconnectedTracker _disconnectedTracker;

        private readonly Metrics _metrics;
        private readonly AckSystem _ackSystem;

        private ConnectionState _state;
        public ConnectionState State
        {
            get => _state;
            set
            {
                // check new state is allowed for current state
                switch (value)
                {
                    case ConnectionState.Connected:
                        _logger?.Assert(_state == ConnectionState.Created || _state == ConnectionState.Connecting);
                        break;

                    case ConnectionState.Connecting:
                        _logger?.Assert(_state == ConnectionState.Created);
                        break;

                    case ConnectionState.Disconnected:
                        _logger?.Assert(_state == ConnectionState.Connected);
                        break;

                    case ConnectionState.Destroyed:
                        _logger?.Assert(_state == ConnectionState.Removing);
                        break;
                }

                if (_logger.Enabled(LogType.Log)) _logger.Log($"{EndPoint} changed state from {_state} to {value}");
                _state = value;
            }
        }

        IEndPoint IConnection.EndPoint => EndPoint;

        public bool Connected => State == ConnectionState.Connected;

        internal Connection(Peer peer, IEndPoint endPoint, IDataHandler dataHandler, Config config, int maxPacketSize, Time time, Pool<ByteBuffer> bufferPool, ILogger logger, Metrics metrics)
        {
            _peer = peer;
            _logger = logger;

            EndPoint = endPoint ?? throw new ArgumentNullException(nameof(endPoint));
            _dataHandler = dataHandler ?? throw new ArgumentNullException(nameof(dataHandler));
            State = ConnectionState.Created;

            _connectingTracker = new ConnectingTracker(config, time);
            _timeoutTracker = new TimeoutTracker(config, time);
            _keepAliveTracker = new KeepAliveTracker(config, time);
            _disconnectedTracker = new DisconnectedTracker(config, time);

            _metrics = metrics;
            _ackSystem = new AckSystem(this, config, maxPacketSize, time, bufferPool, metrics);
        }

        public override string ToString()
        {
            return $"[{EndPoint}]";
        }

        /// <summary>
        /// Checks if new message need to be sent using its <see cref="State"/>
        /// <para>Call this at end of frame to send new batches</para>
        /// </summary>
        public void Update()
        {
            switch (State)
            {
                case ConnectionState.Connecting:
                    UpdateConnecting();
                    break;

                case ConnectionState.Connected:
                    UpdateConnected();
                    break;

                case ConnectionState.Disconnected:
                    UpdateDisconnected();
                    break;
            }
        }

        public void SetReceiveTime()
        {
            _timeoutTracker.SetReceiveTime();
        }
        public void SetSendTime()
        {
            _keepAliveTracker.SetSendTime();
        }

        private void ThrowIfNotConnected()
        {
            if (_state != ConnectionState.Connected)
                throw new InvalidOperationException("Connection is not connected");
        }


        public void SendUnreliable(byte[] packet, int offset, int length)
        {
            ThrowIfNotConnected();
            _metrics?.OnSendMessageUnreliable(length);
            _peer.SendUnreliable(this, packet, offset, length);
        }
        public void SendUnreliable(byte[] packet)
        {
            SendUnreliable(packet, 0, packet.Length);
        }
        public void SendUnreliable(ArraySegment<byte> packet)
        {
            SendUnreliable(packet.Array, packet.Offset, packet.Count);
        }

        /// <summary>
        /// Use <see cref="INotifyCallBack"/> version for non-alloc
        /// </summary>
        public INotifyToken SendNotify(byte[] packet, int offset, int length)
        {
            ThrowIfNotConnected();
            _metrics?.OnSendMessageNotify(length);
            return _ackSystem.SendNotify(packet, offset, length);
        }
        /// <summary>
        /// Use <see cref="INotifyCallBack"/> version for non-alloc
        /// </summary>
        public INotifyToken SendNotify(byte[] packet)
        {
            return SendNotify(packet, 0, packet.Length);
        }
        /// <summary>
        /// Use <see cref="INotifyCallBack"/> version for non-alloc
        /// </summary>
        public INotifyToken SendNotify(ArraySegment<byte> packet)
        {
            return SendNotify(packet.Array, packet.Offset, packet.Count);
        }

        /// <summary>
        /// Use <see cref="INotifyCallBack"/> version for non-alloc
        /// </summary>
        public void SendNotify(byte[] packet, int offset, int length, INotifyCallBack callBacks)
        {
            ThrowIfNotConnected();
            _metrics?.OnSendMessageNotify(length);
            _ackSystem.SendNotify(packet, offset, length, callBacks);
        }
        /// <summary>
        /// Use <see cref="INotifyCallBack"/> version for non-alloc
        /// </summary>
        public void SendNotify(byte[] packet, INotifyCallBack callBacks)
        {
            SendNotify(packet, 0, packet.Length, callBacks);
        }
        /// <summary>
        /// Use <see cref="INotifyCallBack"/> version for non-alloc
        /// </summary>
        public void SendNotify(ArraySegment<byte> packet, INotifyCallBack callBacks)
        {
            SendNotify(packet.Array, packet.Offset, packet.Count, callBacks);
        }


        /// <summary>
        /// single message, batched by AckSystem
        /// </summary>
        /// <param name="message"></param>
        public void SendReliable(byte[] message, int offset, int length)
        {
            ThrowIfNotConnected();
            _metrics?.OnSendMessageReliable(length);
            _ackSystem.SendReliable(message, offset, length);
        }
        public void SendReliable(byte[] packet)
        {
            SendReliable(packet, 0, packet.Length);
        }
        public void SendReliable(ArraySegment<byte> packet)
        {
            SendReliable(packet.Array, packet.Offset, packet.Count);
        }


        void IRawConnection.SendRaw(byte[] packet, int length)
        {
            _peer.Send(this, packet, length);
        }

        /// <summary>
        /// starts disconnecting this connection
        /// </summary>
        public void Disconnect()
        {
            Disconnect(DisconnectReason.RequestedByLocalPeer);
        }
        internal void Disconnect(DisconnectReason reason, bool sendToOther = true)
        {
            if (_logger.Enabled(LogType.Log)) _logger.Log($"Disconnect with reason: {reason}");
            switch (State)
            {
                case ConnectionState.Connecting:
                    _peer.FailedToConnect(this, RejectReason.ClosedByPeer);
                    break;

                case ConnectionState.Connected:
                    State = ConnectionState.Disconnected;
                    _disconnectedTracker.OnDisconnect();
                    _peer.OnConnectionDisconnected(this, reason, sendToOther);
                    break;

                default:
                    break;
            }
        }

        internal void ReceiveUnreliablePacket(Packet packet)
        {
            var offset = 1;
            var count = packet.Length - offset;
            var segment = new ArraySegment<byte>(packet.Buffer.array, offset, count);
            _metrics?.OnReceiveMessageUnreliable(count);
            _dataHandler.ReceiveMessage(this, segment);
        }

        internal void ReceiveReliablePacket(Packet packet)
        {
            _ackSystem.ReceiveReliable(packet.Buffer.array, packet.Length, false);

            HandleQueuedMessages();
        }

        internal void ReceiveReliableFragment(Packet packet)
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
            while (offset < packetLength)
            {
                var length = ByteUtils.ReadUShort(array, ref offset);
                var message = new ArraySegment<byte>(array, offset, length);
                offset += length;

                _metrics?.OnReceiveMessageReliable(length);
                _dataHandler.ReceiveMessage(this, message);
            }

            // release buffer after all its message have been handled
            received.Buffer.Release();
        }

        internal void ReceiveNotifyPacket(Packet packet)
        {
            var segment = _ackSystem.ReceiveNotify(packet.Buffer.array, packet.Length);
            if (segment != default)
            {
                _metrics?.OnReceiveMessageNotify(packet.Length);
                _dataHandler.ReceiveMessage(this, segment);
            }
        }

        internal void ReceiveNotifyAck(Packet packet)
        {
            _ackSystem.ReceiveAck(packet.Buffer.array);
        }


        /// <summary>
        /// client connecting attempts
        /// </summary>
        private void UpdateConnecting()
        {
            if (_connectingTracker.TimeAttempt())
            {
                if (_connectingTracker.MaxAttempts())
                {
                    _peer.FailedToConnect(this, RejectReason.Timeout);
                }
                else
                {
                    _connectingTracker.OnAttempt();
                    _peer.SendConnectRequest(this);
                }
            }
        }

        /// <summary>
        /// Used to remove connection after it has been disconnected
        /// </summary>
        private void UpdateDisconnected()
        {
            if (_disconnectedTracker.TimeToRemove())
            {
                _peer.RemoveConnection(this);
            }
        }

        void IConnection.FlushBatch()
        {
            _ackSystem.Update();
        }

        /// <summary>
        /// Used to keep connection alive
        /// </summary>
        private void UpdateConnected()
        {
            if (_timeoutTracker.TimeToDisconnect())
            {
                Disconnect(DisconnectReason.Timeout);
            }

            _ackSystem.Update();

            if (_keepAliveTracker.TimeToSend())
            {
                _peer.SendKeepAlive(this);
            }
        }

        private class ConnectingTracker
        {
            private readonly Config _config;
            private readonly Time _time;
            private float _lastAttempt = float.MinValue;
            private int _attemptCount = 0;

            public ConnectingTracker(Config config, Time time)
            {
                _config = config;
                _time = time;
            }

            public bool TimeAttempt()
            {
                return _lastAttempt + _config.ConnectAttemptInterval < _time.Now;
            }

            public bool MaxAttempts()
            {
                return _attemptCount >= _config.MaxConnectAttempts;
            }

            public void OnAttempt()
            {
                _attemptCount++;
                _lastAttempt = _time.Now;
            }
        }

        private class TimeoutTracker
        {
            private float _lastRecvTime = float.MinValue;
            private readonly Config _config;
            private readonly Time _time;

            public TimeoutTracker(Config config, Time time)
            {
                _config = config;
                _time = time ?? throw new ArgumentNullException(nameof(time));
            }

            public bool TimeToDisconnect()
            {
                return _lastRecvTime + _config.TimeoutDuration < _time.Now;
            }

            public void SetReceiveTime()
            {
                _lastRecvTime = _time.Now;
            }
        }

        private class KeepAliveTracker
        {
            private float _lastSendTime = float.MinValue;
            private readonly Config _config;
            private readonly Time _time;

            public KeepAliveTracker(Config config, Time time)
            {
                _config = config;
                _time = time ?? throw new ArgumentNullException(nameof(time));
            }


            public bool TimeToSend()
            {
                return _lastSendTime + _config.KeepAliveInterval < _time.Now;
            }

            public void SetSendTime()
            {
                _lastSendTime = _time.Now;
            }
        }

        private class DisconnectedTracker
        {
            private bool _isDisonnected;
            private float _disconnectTime;
            private readonly Config _config;
            private readonly Time _time;

            public DisconnectedTracker(Config config, Time time)
            {
                _config = config;
                _time = time ?? throw new ArgumentNullException(nameof(time));
            }

            public void OnDisconnect()
            {
                _disconnectTime = _time.Now + _config.DisconnectDuration;
                _isDisonnected = true;
            }

            public bool TimeToRemove()
            {
                return _isDisonnected && _disconnectTime < _time.Now;
            }
        }
    }
}
