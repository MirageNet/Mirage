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
        IConnection Connect(EndPoint endPoint);
        void Close();
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
        // list so that remove can take place after foreach loops
        readonly List<Connection> connectionsToRemove = new List<Connection>();

        public event Action<IConnection> OnConnected;
        public event Action<IConnection, DisconnectReason> OnDisconnected;
        public event Action<IConnection, RejectReason> OnConnectionFailed;

        /// <summary>
        /// is server listening on or connected to endpoint
        /// </summary>
        bool active;

        EndPoint receiveEndPoint = null;

        public Peer(ISocket socket, IDataHandler dataHandler, Config config = null, ILogger logger = null)
        {
            this.logger = logger ?? Debug.unityLogger;
            this.config = config ?? new Config();

            this.socket = socket ?? throw new ArgumentNullException(nameof(socket));
            this.dataHandler = dataHandler ?? throw new ArgumentNullException(nameof(dataHandler));
            time = new Time();

            connectKeyValidator = new ConnectKeyValidator();
            bufferPool = new BufferPool(this.config.Mtu, this.config.BufferPoolStartSize, this.config.BufferPoolMaxSize, this.logger);
        }


        public void Bind(EndPoint endPoint)
        {
            if (active) throw new InvalidOperationException("Peer is already active");
            active = true;
            socket.Bind(endPoint);
        }

        public IConnection Connect(EndPoint endPoint)
        {
            if (active) throw new InvalidOperationException("Peer is already active");

            active = true;
            socket.Connect(endPoint);

            Connection connection = CreateNewConnection(endPoint);
            connection.State = ConnectionState.Connecting;

            // update now to send connectRequest command
            connection.Update();
            return connection;
        }

        public void Close()
        {
            if (!active) throw new InvalidOperationException("Peer is not active");
            active = false;

            // send disconnect messages
            foreach (Connection conn in connections.Values)
            {
                conn.Disconnect(DisconnectReason.RequestedByPeer);
            }
            RemoveConnections();

            // close socket
            socket.Close();
        }

        internal void Send(Connection connection, byte[] data, int length)
        {
            // connecting connections can send connect messages so is allowed
            // todo check connected before message are sent from high level
            Assert(connection.State == ConnectionState.Connected || connection.State == ConnectionState.Connecting || connection.State == ConnectionState.Disconnected);

            socket.Send(connection.EndPoint, data, length);
            connection.SetSendTime();
        }

        internal void SendUnreliable(Connection connection, byte[] packet)
        {
            using (ByteBuffer buffer = bufferPool.Take())
            {
                Buffer.BlockCopy(packet, 0, buffer.array, 1, packet.Length);
                // set header
                buffer.array[0] = (byte)PacketType.Unreliable;

                Send(connection, buffer.array, packet.Length + 1);
            }
        }

        internal void SendCommandUnconnected(EndPoint endPoint, Commands command, byte? extra = null)
        {
            using (ByteBuffer buffer = bufferPool.Take())
            {
                int length = CreateCommandPacket(buffer, command, extra);
                socket.Send(endPoint, buffer.array, length);
            }
        }

        internal void SendConnectRequest(Connection connection)
        {
            SendCommand(connection, Commands.ConnectRequest, connectKeyValidator.GetKey());
        }

        internal void SendCommand(Connection connection, Commands command, byte? extra = null)
        {
            using (ByteBuffer buffer = bufferPool.Take())
            {
                int length = CreateCommandPacket(buffer, command, extra);
                Send(connection, buffer.array, length);
            }
        }

        /// <summary>
        /// Create a command packet from command and extra data
        /// </summary>
        /// <param name="buffer">buffer to write to</param>
        /// <param name="command"></param>
        /// <param name="extra">optional extra data as 3rd byte</param>
        /// <returns>length written</returns>
        private int CreateCommandPacket(ByteBuffer buffer, Commands command, byte? extra = null)
        {
            buffer.array[0] = (byte)PacketType.Command;
            buffer.array[1] = (byte)command;

            if (extra.HasValue)
            {
                buffer.array[2] = extra.Value;
                return 3;
            }
            else
            {
                return 2;
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
                    int length = socket.Receive(buffer.array, ref receiveEndPoint);

                    // this should never happen. buffer size is only MTU, if socket returns higher length then it has a bug.
                    if (length > config.Mtu)
                        throw new IndexOutOfRangeException($"Socket returned length above MTU: MTU:{config.Mtu} length:{length}");

                    var packet = new Packet(buffer, length);

                    if (connections.TryGetValue(receiveEndPoint, out Connection connection))
                    {
                        HandleMessage(connection, packet);
                    }
                    else
                    {
                        HandleNewConnection(receiveEndPoint, packet);
                    }

                    // socket might have been closed by message handler
                    if (!active) { break; }
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
                    connection.ReceiveUnreliablePacket(packet);
                    break;
                case PacketType.Notify:
                    connection.ReceiveNotifyPacket(packet);
                    break;
                case PacketType.NotifyAck:
                    connection.ReceiveNotifyAck(packet);
                    break;
                case PacketType.KeepAlive:
                    // do nothing
                    break;
                default:
                    // ignore invalid PacketType
                    // return not break, so that receive time is not set for invalid packet
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

            if (!connectKeyValidator.Validate(packet.buffer.array))
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
                    FailedToConnect(connection, reason);
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
            OnDisconnected?.Invoke(connection, reason);
        }

        internal void FailedToConnect(Connection connection, RejectReason reason)
        {
            if (logger.IsLogTypeAllowed(LogType.Log)) logger.Log($"Connection Failed to connect: {reason}");

            RemoveConnection(connection);

            // tell high level
            OnConnectionFailed?.Invoke(connection, reason);
        }

        internal void RemoveConnection(Connection connection)
        {
            // shouldn't be trying to removed a destroyed connected
            Assert(connection.State != ConnectionState.Destroyed);
            Assert(connection.State != ConnectionState.Removing);

            connection.State = ConnectionState.Removing;
            connectionsToRemove.Add(connection);
        }

        void HandleConnectionDisconnect(Connection connection, Packet packet)
        {
            var reason = (DisconnectReason)packet.buffer.array[2];
            connection.Disconnect(reason, false);
        }

        void UpdateConnections()
        {
            foreach (Connection connection in connections.Values)
            {
                connection.Update();
            }

            RemoveConnections();
        }

        void RemoveConnections()
        {
            if (connectionsToRemove.Count == 0)
                return;

            foreach (Connection connection in connectionsToRemove)
            {
                bool removed = connections.Remove(connection.EndPoint);
                connection.State = ConnectionState.Destroyed;

                // value should be removed from dictionary
                Assert(removed);
            }
            connectionsToRemove.Clear();
        }
    }
}
