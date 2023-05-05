using System;
using Mirage.SocketLayer.ConnectionTrackers;
using UnityEngine;

namespace Mirage.SocketLayer
{
    internal abstract class Connection : IConnection
    {
        protected readonly ILogger _logger;
        protected readonly int _maxPacketSize;
        protected readonly Peer _peer;
        protected readonly IDataHandler _dataHandler;

        public readonly IEndPoint EndPoint;

        protected readonly ConnectingTracker _connectingTracker;
        protected readonly TimeoutTracker _timeoutTracker;
        protected readonly KeepAliveTracker _keepAliveTracker;
        protected readonly DisconnectedTracker _disconnectedTracker;

        protected readonly Metrics _metrics;

        protected ConnectionState _state;
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

        protected Connection(Peer peer, IEndPoint endPoint, IDataHandler dataHandler, Config config, int maxPacketSize, Time time, ILogger logger, Metrics metrics)
        {
            _peer = peer;
            _logger = logger;
            _maxPacketSize = maxPacketSize;

            EndPoint = endPoint ?? throw new ArgumentNullException(nameof(endPoint));
            _dataHandler = dataHandler ?? throw new ArgumentNullException(nameof(dataHandler));
            State = ConnectionState.Created;

            _connectingTracker = new ConnectingTracker(config, time);
            _timeoutTracker = new TimeoutTracker(config, time);
            _keepAliveTracker = new KeepAliveTracker(config, time);
            _disconnectedTracker = new DisconnectedTracker(config, time);

            _metrics = metrics;
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

        protected void ThrowIfNotConnectedOrConnecting()
        {
            // sending to Connecting is also valid
            if (_state != ConnectionState.Connected && _state != ConnectionState.Connecting)
                throw new InvalidOperationException("Connection is not connected");
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

        /// <summary>
        /// Used to keep connection alive
        /// </summary> 
        private void UpdateConnected()
        {
            if (_timeoutTracker.TimeToDisconnect())
            {
                Disconnect(DisconnectReason.Timeout);
            }

            FlushBatch();

            if (_keepAliveTracker.TimeToSend())
            {
                _peer.SendKeepAlive(this);
            }
        }

        public abstract void FlushBatch();

        public abstract void SendReliable(byte[] message, int offset, int length);
        public abstract INotifyToken SendNotify(byte[] packet, int offset, int length);
        public abstract void SendNotify(byte[] packet, int offset, int length, INotifyCallBack callBacks);
        public abstract void SendUnreliable(byte[] packet, int offset, int length);

        internal abstract void ReceiveUnreliablePacket(Packet packet);
        internal abstract void ReceiveNotifyPacket(Packet packet);
        internal abstract void ReceiveReliablePacket(Packet packet);
        internal abstract void ReceiveNotifyAck(Packet packet);
        internal abstract void ReceiveReliableFragment(Packet packet);

        protected void HandleReliableBatched(byte[] array, int offset, int packetLength)
        {
            while (offset < packetLength)
            {
                var length = ByteUtils.ReadUShort(array, ref offset);
                var message = new ArraySegment<byte>(array, offset, length);
                offset += length;

                _metrics?.OnReceiveMessageReliable(length);
                _dataHandler.ReceiveMessage(this, message);
            }
        }

        internal abstract bool IsValidSize(Packet packet);
    }
}
