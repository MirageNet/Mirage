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
        readonly ILogger logger;

        ConnectionState _state;
        public ConnectionState State
        {
            get => _state;
            set
            {
                // check new state is allowed for current state
                switch (value)
                {
                    case ConnectionState.Connected:
                        logger?.Assert(_state == ConnectionState.Created || _state == ConnectionState.Connecting);
                        break;

                    case ConnectionState.Connecting:
                        logger?.Assert(_state == ConnectionState.Created);
                        break;

                    case ConnectionState.Disconnected:
                        logger?.Assert(_state == ConnectionState.Connected);
                        break;

                    case ConnectionState.Destroyed:
                        logger?.Assert(_state == ConnectionState.Removing);
                        break;
                }

                if (logger.Enabled(LogType.Log)) logger.Log($"{EndPoint} changed state from {_state} to {value}");
                _state = value;
            }
        }
        public bool Connected => State == ConnectionState.Connected;

        private readonly Peer peer;
        public readonly IEndPoint EndPoint;
        private readonly IDataHandler dataHandler;

        private readonly ConnectingTracker connectingTracker;
        private readonly TimeoutTracker timeoutTracker;
        private readonly KeepAliveTracker keepAliveTracker;
        private readonly DisconnectedTracker disconnectedTracker;

        private readonly Metrics metrics;
        private readonly AckSystem ackSystem;

        IEndPoint IConnection.EndPoint => EndPoint;

        internal Connection(Peer peer, IEndPoint endPoint, IDataHandler dataHandler, Config config, Time time, Pool<ByteBuffer> bufferPool, ILogger logger, Metrics metrics)
        {
            this.peer = peer;
            this.logger = logger;

            EndPoint = endPoint ?? throw new ArgumentNullException(nameof(endPoint));
            this.dataHandler = dataHandler ?? throw new ArgumentNullException(nameof(dataHandler));
            State = ConnectionState.Created;

            connectingTracker = new ConnectingTracker(config, time);
            timeoutTracker = new TimeoutTracker(config, time);
            keepAliveTracker = new KeepAliveTracker(config, time);
            disconnectedTracker = new DisconnectedTracker(config, time);

            this.metrics = metrics;
            ackSystem = new AckSystem(this, config, time, bufferPool, metrics);
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
            timeoutTracker.SetReceiveTime();
        }
        public void SetSendTime()
        {
            keepAliveTracker.SetSendTime();
        }

        void ThrowIfNotConnected()
        {
            if (_state != ConnectionState.Connected)
                throw new InvalidOperationException("Connection is not connected");
        }


        public void SendUnreliable(byte[] packet, int offset, int length)
        {
            ThrowIfNotConnected();
            metrics?.OnSendMessageUnreliable(length);
            peer.SendUnreliable(this, packet, offset, length);
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
            metrics?.OnSendMessageNotify(length);
            return ackSystem.SendNotify(packet, offset, length);
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
            metrics?.OnSendMessageNotify(length);
            ackSystem.SendNotify(packet, offset, length, callBacks);
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
            metrics?.OnSendMessageReliable(length);
            ackSystem.SendReliable(message, offset, length);
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
            peer.Send(this, packet, length);
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
            if (logger.Enabled(LogType.Log)) logger.Log($"Disconnect with reason: {reason}");
            switch (State)
            {
                case ConnectionState.Connecting:
                    peer.FailedToConnect(this, RejectReason.ClosedByPeer);
                    break;

                case ConnectionState.Connected:
                    State = ConnectionState.Disconnected;
                    disconnectedTracker.OnDisconnect();
                    peer.OnConnectionDisconnected(this, reason, sendToOther);
                    break;

                default:
                    break;
            }
        }

        internal void ReceiveUnreliablePacket(Packet packet)
        {
            int offset = 1;
            int count = packet.length - offset;
            var segment = new ArraySegment<byte>(packet.buffer.array, offset, count);
            metrics?.OnReceiveMessageUnreliable(count);
            dataHandler.ReceiveMessage(this, segment);
        }

        internal void ReceiveReliablePacket(Packet packet)
        {
            ackSystem.ReceiveReliable(packet.buffer.array, packet.length, false);

            HandleQueuedMessages();
        }

        internal void ReceiveReliableFragment(Packet packet)
        {
            if (ackSystem.InvalidFragment(packet.buffer.array))
            {
                Disconnect(DisconnectReason.InvalidPacket);
                return;
            }

            ackSystem.ReceiveReliable(packet.buffer.array, packet.length, true);

            HandleQueuedMessages();
        }

        void HandleQueuedMessages()
        {
            // gets messages in order
            while (ackSystem.NextReliablePacket(out AckSystem.ReliableReceived received))
            {
                if (received.isFragment)
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
            byte[] firstArray = received.buffer.array;
            // length +1 because zero indexed 
            int fragmentLength = firstArray[0] + 1;

            // todo find way to remove allocation? (can't use buffers because they will be too small for this bigger message)
            byte[] message = new byte[fragmentLength * ackSystem.SizePerFragment];

            // copy first
            int copyLength = received.length - 1;
            logger?.Assert(copyLength == ackSystem.SizePerFragment, "First should be max size");
            Buffer.BlockCopy(firstArray, 1, message, 0, copyLength);
            received.buffer.Release();

            int messageLength = copyLength;
            // start at 1 because first copied above
            for (int i = 1; i < fragmentLength; i++)
            {
                AckSystem.ReliableReceived next = ackSystem.GetNextFragment();
                byte[] nextArray = next.buffer.array;

                logger?.Assert(i == (fragmentLength - 1 - nextArray[0]), "fragment index should decrement each time");

                // +1 because first is copied above
                copyLength = next.length - 1;
                Buffer.BlockCopy(nextArray, 1, message, ackSystem.SizePerFragment * i, copyLength);
                messageLength += copyLength;
                next.buffer.Release();
            }

            metrics?.OnReceiveMessageReliable(messageLength);
            dataHandler.ReceiveMessage(this, new ArraySegment<byte>(message, 0, messageLength));
        }

        private void HandleBatchedMessageInPacket(AckSystem.ReliableReceived received)
        {
            byte[] array = received.buffer.array;
            int packetLength = received.length;
            int offset = 0;
            while (offset < packetLength)
            {
                ushort length = ByteUtils.ReadUShort(array, ref offset);
                var message = new ArraySegment<byte>(array, offset, length);
                offset += length;

                metrics?.OnReceiveMessageReliable(length);
                dataHandler.ReceiveMessage(this, message);
            }

            // release buffer after all its message have been handled
            received.buffer.Release();
        }

        internal void ReceiveNotifyPacket(Packet packet)
        {
            ArraySegment<byte> segment = ackSystem.ReceiveNotify(packet.buffer.array, packet.length);
            if (segment != default)
            {
                metrics?.OnReceiveMessageNotify(packet.length);
                dataHandler.ReceiveMessage(this, segment);
            }
        }

        internal void ReceiveNotifyAck(Packet packet)
        {
            ackSystem.ReceiveAck(packet.buffer.array);
        }


        /// <summary>
        /// client connecting attempts
        /// </summary>
        void UpdateConnecting()
        {
            if (connectingTracker.TimeAttempt())
            {
                if (connectingTracker.MaxAttempts())
                {
                    peer.FailedToConnect(this, RejectReason.Timeout);
                }
                else
                {
                    connectingTracker.OnAttempt();
                    peer.SendConnectRequest(this);
                }
            }
        }

        /// <summary>
        /// Used to remove connection after it has been disconnected
        /// </summary>
        void UpdateDisconnected()
        {
            if (disconnectedTracker.TimeToRemove())
            {
                peer.RemoveConnection(this);
            }
        }

        /// <summary>
        /// Used to keep connection alive
        /// </summary>
        void UpdateConnected()
        {
            if (timeoutTracker.TimeToDisconnect())
            {
                Disconnect(DisconnectReason.Timeout);
            }

            ackSystem.Update();

            if (keepAliveTracker.TimeToSend())
            {
                peer.SendKeepAlive(this);
            }
        }

        class ConnectingTracker
        {
            private readonly Config config;
            private readonly Time time;
            float lastAttempt = float.MinValue;
            int AttemptCount = 0;

            public ConnectingTracker(Config config, Time time)
            {
                this.config = config;
                this.time = time;
            }

            public bool TimeAttempt()
            {
                return lastAttempt + config.ConnectAttemptInterval < time.Now;
            }

            public bool MaxAttempts()
            {
                return AttemptCount >= config.MaxConnectAttempts;
            }

            public void OnAttempt()
            {
                AttemptCount++;
                lastAttempt = time.Now;
            }
        }
        class TimeoutTracker
        {
            float lastRecvTime = float.MinValue;
            readonly Config config;
            readonly Time time;

            public TimeoutTracker(Config config, Time time)
            {
                this.config = config;
                this.time = time ?? throw new ArgumentNullException(nameof(time));
            }

            public bool TimeToDisconnect()
            {
                return lastRecvTime + config.TimeoutDuration < time.Now;
            }

            public void SetReceiveTime()
            {
                lastRecvTime = time.Now;
            }
        }
        class KeepAliveTracker
        {
            float lastSendTime = float.MinValue;
            readonly Config config;
            readonly Time time;

            public KeepAliveTracker(Config config, Time time)
            {
                this.config = config;
                this.time = time ?? throw new ArgumentNullException(nameof(time));
            }


            public bool TimeToSend()
            {
                return lastSendTime + config.KeepAliveInterval < time.Now;
            }

            public void SetSendTime()
            {
                lastSendTime = time.Now;
            }
        }
        class DisconnectedTracker
        {
            bool isDisonnected;
            float disconnectTime;
            readonly Config config;
            readonly Time time;

            public DisconnectedTracker(Config config, Time time)
            {
                this.config = config;
                this.time = time ?? throw new ArgumentNullException(nameof(time));
            }

            public void OnDisconnect()
            {
                disconnectTime = time.Now + config.DisconnectDuration;
                isDisonnected = true;
            }

            public bool TimeToRemove()
            {
                return isDisonnected && disconnectTime < time.Now;
            }
        }
    }
}
