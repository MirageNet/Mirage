using System;
using System.Net;
using UnityEngine;

namespace Mirage.SocketLayer
{
    public enum ConnectionState
    {
        /// <summary>
        /// Initial state
        /// </summary>
        Created = 1,
        /// <summary>
        /// Client is connecting to server
        /// </summary>
        Connecting = 2,
        /// <summary>
        /// Server as accepted connection
        /// </summary>
        Connected = 3,

        Disconnected = 9,
        Destroyed = 10,
    }

    public interface IConnectionPlayer
    {
        Connection Connection { get; }
    }

    public sealed class Connection
    {
        void Assert(bool condition)
        {
            if (!condition) logger.Log(LogType.Assert, "Failed Assertion");
        }
        readonly ILogger logger;

        public ConnectionState _state;
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
                }

                if (logger.IsLogTypeAllowed(LogType.Log)) logger.Log($"{EndPoint} changed state from {_state} to {value}");
                _state = value;
            }
        }

        private readonly Peer peer;
        public readonly EndPoint EndPoint;
        private readonly IDataHandler dataHandler;
        private readonly Config config;
        private readonly Time time;

        private ConnectingTracker connectingTracker;
        private TimeoutTracker timeoutTracker;
        private KeepAliveTracker keepAliveTracker;
        private DisconnectedTracker disconnectedTracker;

        IConnectionPlayer player;

        internal Connection(Peer peer, EndPoint endPoint, IDataHandler dataHandler, Config config, Time time, ILogger logger)
        {
            this.peer = peer;
            this.logger = logger ?? Debug.unityLogger;
            EndPoint = endPoint ?? throw new ArgumentNullException(nameof(endPoint));
            this.dataHandler = dataHandler ?? throw new ArgumentNullException(nameof(dataHandler));
            this.config = config;
            this.time = time;
            State = ConnectionState.Created;

            connectingTracker = new ConnectingTracker(config, time);
            timeoutTracker = new TimeoutTracker(config, time);
            keepAliveTracker = new KeepAliveTracker(config, time);
            disconnectedTracker = new DisconnectedTracker(config, time);
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

        public void SendReliable(ArraySegment<byte> segment) => peer.SendReliable(this);
        public void SendUnreiable(ArraySegment<byte> segment) => peer.SendUnreliable(this);
        public void SendNotifiy() => peer.SendNotify(this);


        public void AddPlayer(IConnectionPlayer player)
        {
            if (this.player != null) throw new InvalidOperationException("Cant set player if one is already set");

            this.player = player;
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
            State = ConnectionState.Disconnected;
            disconnectedTracker.Disconnect();
            if (sendToOther)
                peer.SendCommand(this, Commands.Disconnect, (byte)reason);
        }

        internal void ReceivePacket(Packet packet)
        {
            ArraySegment<byte> segment = packet.ToSegment();
            dataHandler.ReceiveData(player, segment);
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
                    peer.RemoveConnection(this);
                }

                connectingTracker.OnAttempt();
                peer.SendCommand(this, Commands.ConnectRequest);
            }
        }
        void UpdateDisconnected()
        {
            if (disconnectedTracker.TimeToRemove())
            {
                peer.RemoveConnection(this);
            }
        }

        /// <summary>
        /// Used
        /// </summary>
        void UpdateConnected()
        {
            if (timeoutTracker.TimeToDisconnect())
            {
                // disconnect here
                throw new NotImplementedException();
                return;
                Disconnect(DisconnectReason.Timeout);
            }

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

            public void Disconnect()
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
