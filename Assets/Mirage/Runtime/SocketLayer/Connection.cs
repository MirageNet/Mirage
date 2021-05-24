using System;
using System.Net;
using UnityEngine;

namespace Mirage.SocketLayer
{
    /// <summary>
    /// Connection for <see cref="Peer"/>
    /// </summary>
    public interface IConnection
    {
        EndPoint EndPoint { get; }
        ConnectionState State { get; }

        void Disconnect();

        INotifyToken SendNotify(byte[] packet);
        /// <summary>
        /// single message, batched by AckSystem
        /// </summary>
        /// <param name="message"></param>
        void SendReliable(byte[] message);
        void SendUnreliable(byte[] packet);
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
        void Assert(bool condition, object msg = null)
        {
            if (!condition) logger.Log(LogType.Assert, msg == null ? "Failed Assertion" : $"Failed Assertion: {msg}");
        }
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
                        Assert(_state == ConnectionState.Created || _state == ConnectionState.Connecting);
                        break;

                    case ConnectionState.Connecting:
                        Assert(_state == ConnectionState.Created);
                        break;

                    case ConnectionState.Disconnected:
                        Assert(_state == ConnectionState.Connected);
                        break;

                    case ConnectionState.Destroyed:
                        Assert(_state == ConnectionState.Removing);
                        break;
                }

                if (logger.IsLogTypeAllowed(LogType.Log)) logger.Log($"{EndPoint} changed state from {_state} to {value}");
                _state = value;
            }
        }
        public bool Connected => State == ConnectionState.Connected;

        private readonly Peer peer;
        public readonly EndPoint EndPoint;
        private readonly IDataHandler dataHandler;

        private readonly ConnectingTracker connectingTracker;
        private readonly TimeoutTracker timeoutTracker;
        private readonly KeepAliveTracker keepAliveTracker;
        private readonly DisconnectedTracker disconnectedTracker;

        private readonly AckSystem ackSystem;

        EndPoint IConnection.EndPoint => EndPoint;

        internal Connection(Peer peer, EndPoint endPoint, IDataHandler dataHandler, Config config, Time time, BufferPool bufferPool, ILogger logger, Metrics metrics)
        {
            this.peer = peer;
            this.logger = logger ?? Debug.unityLogger;
            EndPoint = endPoint ?? throw new ArgumentNullException(nameof(endPoint));
            this.dataHandler = dataHandler ?? throw new ArgumentNullException(nameof(dataHandler));
            State = ConnectionState.Created;

            connectingTracker = new ConnectingTracker(config, time);
            timeoutTracker = new TimeoutTracker(config, time);
            keepAliveTracker = new KeepAliveTracker(config, time);
            disconnectedTracker = new DisconnectedTracker(config, time);

            ackSystem = new AckSystem(this, config, time, bufferPool, metrics);
        }

        public override string ToString()
        {
            return $"[{EndPoint}]";
        }

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
        public void SendUnreliable(byte[] packet)
        {
            ThrowIfNotConnected();
            peer.SendUnreliable(this, packet);
        }

        public INotifyToken SendNotify(byte[] packet)
        {
            ThrowIfNotConnected();
            return ackSystem.SendNotify(packet);
        }
        /// <summary>
        /// single message, batched by AckSystem
        /// </summary>
        /// <param name="message"></param>
        public void SendReliable(byte[] message)
        {
            ThrowIfNotConnected();
            ackSystem.SendReliable(message);
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
            Disconnect(DisconnectReason.RequestedByPeer);
        }
        internal void Disconnect(DisconnectReason reason, bool sendToOther = true)
        {
            if (logger.filterLogType == LogType.Log) logger.Log($"Disconnect with reason: {reason}");
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
            dataHandler.ReceiveMessage(this, segment);
        }

        internal void ReceivReliablePacket(Packet packet)
        {
            ackSystem.ReceiveReliable(packet.buffer.array, packet.length);

            // gets messages in order
            while (ackSystem.NextReliablePacket(out AckSystem.ReliableReceived received))
            {
                HandleAllMessageInPacket(received);
            }
        }

        private void HandleAllMessageInPacket(AckSystem.ReliableReceived received)
        {
            byte[] array = received.buffer.array;
            int packetLength = received.length;
            int offset = 0;
            while (offset < packetLength)
            {
                ushort length = ByteUtils.ReadUShort(array, ref offset);
                var message = new ArraySegment<byte>(array, offset, length);
                offset += length;

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
