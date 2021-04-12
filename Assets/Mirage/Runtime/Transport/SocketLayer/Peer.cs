using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

namespace Mirage.SocketLayer
{
    internal class Time
    {
        public float Now => UnityEngine.Time.time;
    }

    public interface IPeer
    {
        event Action<IConnection> OnConnected;
        event Action<IConnection, RejectReason> OnConnectionFailed;
        event Action<IConnection, DisconnectReason> OnDisconnected;

        void Bind(EndPoint endPoint);
        void Close();
        IConnection Connect(EndPoint endPoint);
        void Update();
    }

    /// <summary>
    /// Controls flow of data in/out of mirage, Uses <see cref="ISocket"/>
    /// </summary>
    public sealed class Peer : IPeer
    {
        void Assert(bool condition)
        {
            if (!condition) logger.Log(LogType.Assert, "Failed Assertion");
        }
        void Error(string error)
        {
            logger.Log(LogType.Error, error);
        }
        readonly ILogger logger;

        readonly ISocket socket;
        readonly IDataHandler dataHandler;
        readonly Config config;
        readonly Time time;

        readonly ConnectKeyValidator connectKeyValidator;
        readonly BufferPool bufferPool;
        readonly Dictionary<EndPoint, Connection> connections = new Dictionary<EndPoint, Connection>();

        public event Action<IConnection> OnConnected;
        public event Action<IConnection, DisconnectReason> OnDisconnected;
        public event Action<IConnection, RejectReason> OnConnectionFailed;

        bool active;

        public Peer(ISocket socket, IDataHandler dataHandler, Config config = null, ILogger logger = null)
        {
            this.logger = logger ?? Debug.unityLogger;
            this.socket = socket ?? throw new ArgumentNullException(nameof(socket));
            this.dataHandler = dataHandler ?? throw new ArgumentNullException(nameof(dataHandler));
            this.config = config ?? new Config();
            time = new Time();

            connectKeyValidator = new ConnectKeyValidator();
            bufferPool = new BufferPool(config.Mtu, config.BufferPoolStartSize, config.BufferPoolMaxSize, logger);
        }


        public void Bind(EndPoint endPoint)
        {
            socket.Bind(endPoint);
            active = true;
        }

        public IConnection Connect(EndPoint endPoint)
        {
            Connection connection = CreateNewConnection(endPoint);
            connection.State = ConnectionState.Connecting;

            // update now to send connectRequest command

            connection.Update();
            active = true;
            return connection;
        }

        public void Close()
        {
            if (!active) throw new InvalidOperationException("Peer is not active");

            // send disconnect messages
            foreach (Connection conn in connections.Values)
            {
                conn.Disconnect(DisconnectReason.RequestedByPeer);
            }

            active = false;
            // close socket
            socket.Close();
        }

        // todo SendNotify
        internal void SendNotify(Connection connection) => throw new NotImplementedException();
        // todo SendReliable
        internal void SendReliable(Connection connection) => throw new NotImplementedException();

        internal void SendUnreliable(Connection connection, ArraySegment<byte> message)
        {
            using (ByteBuffer buffer = bufferPool.Take())
            {
                Buffer.BlockCopy(message.Array, message.Offset, buffer.array, 1, message.Count);
                // set header
                buffer.array[0] = (byte)PacketType.Unreliable;

                Send(connection, buffer.array, message.Count);
            }
        }
        internal void SendRaw(Connection connection, byte[] packet)
        {
            // todo asset header is command?
            Send(connection, packet);
        }

        private void Send(Connection connection, Packet packet) => Send(connection, packet.buffer.array, packet.length);
        private void Send(Connection connection, byte[] data, int? length = null)
        {
            // todo check connection state before sending
            socket.Send(connection.EndPoint, data, length);
            connection.SetSendTime();
        }
        private void SendUnconnected(EndPoint endPoint, Packet packet) => SendUnconnected(endPoint, packet.buffer.array, packet.length);
        internal void SendUnconnected(EndPoint endPoint, byte[] data, int? length = null)
        {
            socket.Send(endPoint, data, length);
        }

        internal void SendCommandUnconnected(EndPoint endPoint, Commands command, byte? extra = null)
        {
            Packet packet = CreateCommandPacket(command, extra);
            SendUnconnected(endPoint, packet);
        }

        internal void SendConnectRequest(Connection connection)
        {
            SendCommand(connection, Commands.ConnectRequest, connectKeyValidator.GetKey());
        }
        internal void SendCommand(Connection connection, Commands command, byte? extra = null)
        {
            Packet packet = CreateCommandPacket(command, extra);
            Send(connection, packet);
        }
        private Packet CreateCommandPacket(Commands command, byte? extra = null)
        {
            using (ByteBuffer buffer = bufferPool.Take())
            {
                buffer.array[0] = (byte)PacketType.Command;
                buffer.array[1] = (byte)command;

                if (extra.HasValue)
                {
                    buffer.array[2] = extra.Value;
                    return new Packet(buffer, 3);
                }
                else
                {
                    return new Packet(buffer, 2);
                }
            }
        }

        internal void SendKeepAlive(Connection connection)
        {
            using (ByteBuffer buffer = bufferPool.Take())
            {
                buffer.array[0] = (byte)PacketType.KeepAlive;
                Send(connection, buffer.array, 1);
            }
        }

        public void Update()
        {
            ReceiveLoop();
            UpdateConnections();
        }

        private void ReceiveLoop()
        {
            using (ByteBuffer buffer = bufferPool.Take())
            {
                while (socket.Poll())
                {
                    //todo do we need to pass in endpoint?
                    EndPoint endPoint = null;
                    socket.Receive(buffer.array, ref endPoint, out int length);

                    var packet = new Packet(buffer, length);

                    if (connections.TryGetValue(endPoint, out Connection connection))
                    {
                        HandleMessage(connection, packet);
                    }
                    else
                    {
                        HandleNewConnection(endPoint, packet);
                    }
                }
            }
        }

        private void HandleMessage(Connection connection, Packet packet)
        {
            // ingore message of invalid size
            if (!packet.IsValidSize()) { return; }

            switch (packet.type)
            {
                case PacketType.Command:
                    HandleCommand(connection, packet);
                    break;
                case PacketType.Unreliable:
                case PacketType.Reliable:
                case PacketType.Notify:
                    // todo are these handled differently?
                    connection.ReceivePacket(packet);
                    break;
                case PacketType.KeepAlive:
                    // do nothing
                    break;
                default:
                    // ignore invalid PacketType
                    // return not break, so that recieve time is not set for invalid packet
                    return;
            }

            connection.SetReceiveTime();
        }

        private void HandleCommand(Connection connection, Packet packet)
        {
            switch (packet.command)
            {
                case Commands.ConnectRequest:
                    HandleConnectionRequest(connection);
                    break;
                case Commands.ConnectionAccepted:
                    HandleConnectionAccepted(connection);
                    break;
                case Commands.ConnectionRejected:
                    HandleConnectionRejected(connection, packet);
                    break;
                case Commands.Disconnect:
                    HandleConnectionDisconnect(connection, packet);
                    break;
                default:
                    // ignore invalid command
                    break;
            }
        }


        private void HandleNewConnection(EndPoint endPoint, Packet packet)
        {
            // if invalid, then reject without reason
            if (!Validate(packet)) { return; }

            if (AtMaxConnections())
            {
                RejectConnectionWithReason(endPoint, RejectReason.ServerFull);
            }
            else
            {
                AcceptNewConnection(endPoint);
            }
        }

        private bool Validate(Packet packet)
        {
            int requestLength = 2 + connectKeyValidator.KeyLength;
            if (packet.length < requestLength)
                return false;

            // todo do security stuff here:
            // - connect request
            // - simple key/phrase send from client with first message
            // - hashcash??
            // - white/black list for endpoint?

            if (packet.type != PacketType.Command)
                return false;

            if (packet.command != Commands.ConnectRequest)
                return false;

            if (!connectKeyValidator.Validate(packet))
                return false;

            //if (!hashCashValidator.Validate(packet))
            //    return false;

            return true;
        }

        private bool AtMaxConnections()
        {
            return connections.Count >= config.MaxConnections;
        }
        private void AcceptNewConnection(EndPoint endPoint)
        {
            if (logger.IsLogTypeAllowed(LogType.Log)) logger.Log($"Accepting new connection from:{endPoint}");

            Connection connection = CreateNewConnection(endPoint);

            HandleConnectionRequest(connection);
        }

        private Connection CreateNewConnection(EndPoint endPoint)
        {
            var connection = new Connection(this, endPoint, dataHandler, config, time, logger);
            connection.SetReceiveTime();
            connections.Add(endPoint, connection);
            return connection;
        }

        private void HandleConnectionRequest(Connection connection)
        {
            switch (connection.State)
            {
                case ConnectionState.Created:
                    SetConnectionAsConnected(connection);
                    SendCommand(connection, Commands.ConnectionAccepted);
                    break;

                case ConnectionState.Connected:
                    // send command again, unreliable so first message could have been missed
                    SendCommand(connection, Commands.ConnectionAccepted);
                    break;

                case ConnectionState.Connecting:
                    Error($"Server connections should not be in {nameof(ConnectionState.Connecting)} state");
                    break;
            }
        }

        private void SetConnectionAsConnected(Connection connection)
        {
            connection.State = ConnectionState.Connected;
            OnConnected?.Invoke(connection);
        }

        private void RejectConnectionWithReason(EndPoint endPoint, RejectReason reason)
        {
            SendCommandUnconnected(endPoint, Commands.ConnectionRejected, (byte)reason);
        }


        void HandleConnectionAccepted(Connection connection)
        {
            switch (connection.State)
            {
                case ConnectionState.Created:
                    Error($"Accepted Connections should not be in {nameof(ConnectionState.Created)} state");
                    break;

                case ConnectionState.Connected:
                    // ignore this, command may have been re-sent or Received twice
                    break;

                case ConnectionState.Connecting:
                    SetConnectionAsConnected(connection);
                    break;
            }
        }
        void HandleConnectionRejected(Connection connection, Packet packet)
        {
            switch (connection.State)
            {
                case ConnectionState.Connecting:
                    var reason = (RejectReason)packet.buffer.array[2];
                    if (logger.IsLogTypeAllowed(LogType.Log)) logger.Log($"Connection Refused: {reason}");
                    RemoveConnection(connection);
                    OnConnectionFailed?.Invoke(connection, reason);
                    break;

                default:
                    Error($"Rejected Connections should not be in {nameof(ConnectionState.Created)} state");
                    break;
            }
        }

        /// <summary>
        /// Called by connection when it is disconnected
        /// </summary>
        internal void OnConnectionDisconnected(Connection connection, DisconnectReason reason, bool sendToOther)
        {
            if (sendToOther)
            {
                SendCommand(connection, Commands.Disconnect, (byte)reason);
            }

            // tell high level
            OnDisconnected.Invoke(connection, reason);
        }
        internal void RemoveConnection(Connection connection)
        {
            // shouldn't be trying to removed a destroyed connected
            Assert(connection.State != ConnectionState.Destroyed);

            bool removed = connections.Remove(connection.EndPoint);
            connection.State = ConnectionState.Destroyed;

            // value should be removed from dictionary
            Assert(removed);
        }

        void HandleConnectionDisconnect(Connection connection, Packet packet)
        {
            var reason = (DisconnectReason)packet.buffer.array[2];
            connection.Disconnect(reason, false);
        }

        void UpdateConnections()
        {
            foreach (KeyValuePair<EndPoint, Connection> kvp in connections)
            {
                kvp.Value.Update();
            }
        }
    }
}
