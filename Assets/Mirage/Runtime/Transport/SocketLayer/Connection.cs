using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;

namespace Mirage.SocketLayer
{
    public enum ConnectionState
    {
        /// <summary>
        /// Inital state
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
        readonly ILogger logger;

        public ConnectionState State { get; private set; }

        private readonly Peer peer;
        public readonly EndPoint EndPoint;
        private readonly IDataHandler dataHandler;
        private readonly Config config;
        private readonly Time time;

        private ConnectingTracker connectingTracker;
        private TimeoutTracker timeoutTracker;
        private KeepAliveTracker keepAliveTracker;
        private DisconnectedTracker disconnectedTracker;

        HashSet<IConnectionPlayer> players = new HashSet<IConnectionPlayer>();

        public IReadOnlyCollection<IConnectionPlayer> Players => players;

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
            disconnectedTracker = new DisconnectedTracker();
        }

        public void ChangeState(ConnectionState state)
        {
            switch (state)
            {
                case ConnectionState.Connected:
                    if (State == ConnectionState.Created || State == ConnectionState.Connecting) logger.Log(LogType.Assert, "Failed Assertion");
                    break;

                case ConnectionState.Connecting:
                    if (State == ConnectionState.Created) logger.Log(LogType.Assert, "Failed Assertion");
                    break;

                case ConnectionState.Disconnected:
                    if (State == ConnectionState.Connected) logger.Log(LogType.Assert, "Failed Assertion");
                    break;
            }

            if (logger.IsLogTypeAllowed(LogType.Log)) logger.Log($"{EndPoint} changed state from {State} to {state}");
            State = state;
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
            players.Add(player);
        }
        public void DisconnectPlayer(IConnectionPlayer player)
        {
            players.Remove(player);
            if (players.Count == 0)
            {
                Disconnect();
            }
        }

        internal void Disconnect()
        {
            peer.RemoveConnection(this);
        }

        internal void ReceivePacket(Packet packet)
        {
            ArraySegment<byte> segment = packet.ToSegment();
            // todo what if no players?
            dataHandler.ReceiveData(players.First(), segment);
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
                    // client failed to connect
                }

                connectingTracker.OnAttempt();
                peer.SendCommand(this, Commands.ConnectRequest);
            }
        }
        void UpdateDisconnected()
        {
            // todo why not remove disconnected right away
            //if ((connection.DisconnectTime + _config.DisconnectIdleTime) < _clock.ElapsedInSeconds)
            //{
            //    RemoveConnection(connection);
            //}
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

            internal bool MaxAttempts()
            {
                return AttemptCount >= config.MaxConnectAttempts;
            }

            internal void OnAttempt()
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
                return lastRecvTime + config.DisconnectTimeout < time.Now;
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

            internal void SetSendTime()
            {
                lastSendTime = time.Now;
            }
        }
        class DisconnectedTracker { }
    }
}
