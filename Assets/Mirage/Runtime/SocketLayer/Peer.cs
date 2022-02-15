using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mirage.SocketLayer
{
    public interface ITime
    {
        float Now { get; }
    }
    internal class Time : ITime
    {
        public float Now => UnityEngine.Time.time;
    }

    public interface IPeer
    {
        event Action<IConnection> OnConnected;
        event Action<IConnection, RejectReason> OnConnectionFailed;
        event Action<IConnection, DisconnectReason> OnDisconnected;

        void Bind(IEndPoint endPoint);
        IConnection Connect(IEndPoint endPoint);
        void Close();
        /// <summary>
        /// Call this at the start of the frame to receive new messages
        /// </summary>
        void UpdateReceive();
        /// <summary>
        /// Call this at end of frame to send new batches
        /// </summary>
        void UpdateSent();
    }

    /// <summary>
    /// Controls flow of data in/out of mirage, Uses <see cref="ISocket"/>
    /// </summary>
    public sealed class Peer : IPeer
    {
        readonly ILogger logger;
        readonly Metrics metrics;
        readonly ISocket socket;
        readonly IDataHandler dataHandler;
        readonly Config config;
        readonly Time time;

        readonly ConnectKeyValidator connectKeyValidator;
        readonly Pool<ByteBuffer> bufferPool;
        readonly Dictionary<IEndPoint, Connection> connections = new Dictionary<IEndPoint, Connection>();
        // list so that remove can take place after foreach loops
        readonly List<Connection> connectionsToRemove = new List<Connection>();

        public event Action<IConnection> OnConnected;
        public event Action<IConnection, DisconnectReason> OnDisconnected;
        public event Action<IConnection, RejectReason> OnConnectionFailed;

        /// <summary>
        /// is server listening on or connected to endpoint
        /// </summary>
        bool active;

        public Peer(ISocket socket, IDataHandler dataHandler, Config config = null, ILogger logger = null, Metrics metrics = null)
        {
            this.logger = logger;
            this.metrics = metrics;
            this.config = config ?? new Config();

            this.socket = socket ?? throw new ArgumentNullException(nameof(socket));
            this.dataHandler = dataHandler ?? throw new ArgumentNullException(nameof(dataHandler));
            time = new Time();

            connectKeyValidator = new ConnectKeyValidator(this.config.key);

            bufferPool = new Pool<ByteBuffer>(ByteBuffer.CreateNew, this.config.MaxPacketSize, this.config.BufferPoolStartSize, this.config.BufferPoolMaxSize, this.logger);
            Application.quitting += Application_quitting;
        }

        private void Application_quitting()
        {
            // make sure peer closes itself when applications closes.
            // this will make sure that disconnect Command is sent before applications closes
            if (active)
                Close();
        }

        public void Bind(IEndPoint endPoint)
        {
            if (active) throw new InvalidOperationException("Peer is already active");
            active = true;
            socket.Bind(endPoint);
        }

        public IConnection Connect(IEndPoint endPoint)
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
            if (!active)
            {
                if (logger.Enabled(LogType.Warning)) logger.Log(LogType.Warning, "Peer is not active");
                return;
            }
            active = false;
            Application.quitting -= Application_quitting;

            // send disconnect messages
            foreach (Connection conn in connections.Values)
            {
                conn.Disconnect(DisconnectReason.RequestedByLocalPeer);
            }
            RemoveConnections();

            // close socket
            socket.Close();
        }

        internal void Send(Connection connection, byte[] data, int length)
        {
            // connecting connections can send connect messages so is allowed
            // todo check connected before message are sent from high level
            logger?.Assert(connection.State == ConnectionState.Connected || connection.State == ConnectionState.Connecting || connection.State == ConnectionState.Disconnected, connection.State);

            socket.Send(connection.EndPoint, data, length);
            metrics?.OnSend(length);
            connection.SetSendTime();

            if (logger.Enabled(LogType.Log))
            {
                if ((PacketType)data[0] == PacketType.Command)
                {
                    logger.Log($"Send to {connection} type: Command, {(Commands)data[1]}");
                }
                else
                {
                    logger.Log($"Send to {connection} type: {(PacketType)data[0]}");
                }
            }
        }

        internal void SendUnreliable(Connection connection, byte[] packet, int offset, int length)
        {
            using (ByteBuffer buffer = bufferPool.Take())
            {
                Buffer.BlockCopy(packet, offset, buffer.array, 1, length);
                // set header
                buffer.array[0] = (byte)PacketType.Unreliable;

                Send(connection, buffer.array, length + 1);
            }
        }

        internal void SendCommandUnconnected(IEndPoint endPoint, Commands command, byte? extra = null)
        {
            using (ByteBuffer buffer = bufferPool.Take())
            {
                int length = CreateCommandPacket(buffer, command, extra);

                socket.Send(endPoint, buffer.array, length);
                metrics?.OnSendUnconnected(length);
                if (logger.Enabled(LogType.Log))
                {
                    logger.Log($"Send to {endPoint} type: Command, {command}");
                }
            }
        }

        internal void SendConnectRequest(Connection connection)
        {
            using (ByteBuffer buffer = bufferPool.Take())
            {
                int length = CreateCommandPacket(buffer, Commands.ConnectRequest, null);
                connectKeyValidator.CopyTo(buffer.array);
                Send(connection, buffer.array, length + connectKeyValidator.KeyLength);
            }
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

        /// <summary>
        /// Call this at the start of the frame to receive new messages
        /// </summary>
        public void UpdateReceive()
        {
            ReceiveLoop();
        }
        /// <summary>
        /// Call this at end of frame to send new batches
        /// </summary>
        public void UpdateSent()
        {
            UpdateConnections();
            metrics?.OnTick(connections.Count);
        }


        private void ReceiveLoop()
        {
            using (ByteBuffer buffer = bufferPool.Take())
            {
                while (socket.Poll())
                {
                    int length = socket.Receive(buffer.array, out IEndPoint receiveEndPoint);

                    // this should never happen. buffer size is only MTU, if socket returns higher length then it has a bug.
                    if (length > config.MaxPacketSize)
                        throw new IndexOutOfRangeException($"Socket returned length above MTU. MaxPacketSize:{config.MaxPacketSize} length:{length}");

                    var packet = new Packet(buffer, length);

                    if (connections.TryGetValue(receiveEndPoint, out Connection connection))
                    {
                        metrics?.OnReceive(length);
                        HandleMessage(connection, packet);
                    }
                    else
                    {
                        metrics?.OnReceiveUnconnected(length);
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
            if (!packet.IsValidSize())
            {
                if (logger.Enabled(LogType.Log))
                {
                    logger.Log($"Receive from {connection} was too small");
                }
                return;
            }

            if (logger.Enabled(LogType.Log))
            {
                if (packet.type == PacketType.Command)
                {
                    logger.Log($"Receive from {connection} type: Command, {packet.command}");
                }
                else
                {
                    logger.Log($"Receive from {connection} type: {packet.type}");
                }
            }

            if (!connection.Connected)
            {
                // if not connected then we can only handle commands
                if (packet.type == PacketType.Command)
                {
                    HandleCommand(connection, packet);
                    connection.SetReceiveTime();

                }
                else if (logger.Enabled(LogType.Warning)) logger.Log(LogType.Warning, $"Receive from {connection} type: {packet.type} while not connected");

                // ignore other messages if not connected
                return;
            }

            // handle message when connected
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
                case PacketType.Reliable:
                    connection.ReceiveReliablePacket(packet);
                    break;
                case PacketType.Ack:
                    connection.ReceiveNotifyAck(packet);
                    break;
                case PacketType.ReliableFragment:
                    connection.ReceiveReliableFragment(packet);
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

        private void HandleNewConnection(IEndPoint endPoint, Packet packet)
        {
            // if invalid, then reject without reason
            if (!Validate(packet)) { return; }


            if (!connectKeyValidator.Validate(packet.buffer.array))
            {
                RejectConnectionWithReason(endPoint, RejectReason.KeyInvalid);
            }
            else if (AtMaxConnections())
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

            //if (!hashCashValidator.Validate(packet))
            //    return false;

            return true;
        }

        private bool AtMaxConnections()
        {
            return connections.Count >= config.MaxConnections;
        }
        private void AcceptNewConnection(IEndPoint endPoint)
        {
            if (logger.Enabled(LogType.Log)) logger.Log($"Accepting new connection from:{endPoint}");

            Connection connection = CreateNewConnection(endPoint);

            HandleConnectionRequest(connection);
        }

        private Connection CreateNewConnection(IEndPoint _newEndPoint)
        {
            // create copy of endpoint for this connection
            // this is so that we can re-use the endpoint (reduces alloc) for receive and not worry about changing internal data needed for each connection
            IEndPoint endPoint = _newEndPoint?.CreateCopy();

            var connection = new Connection(this, endPoint, dataHandler, config, time, bufferPool, logger, metrics);
            connection.SetReceiveTime();
            connections.Add(endPoint, connection);
            return connection;
        }

        private void HandleConnectionRequest(Connection connection)
        {
            switch (connection.State)
            {
                case ConnectionState.Created:
                    // mark as connected, send message, then invoke event
                    connection.State = ConnectionState.Connected;
                    SendCommand(connection, Commands.ConnectionAccepted);
                    OnConnected?.Invoke(connection);
                    break;

                case ConnectionState.Connected:
                    // send command again, unreliable so first message could have been missed
                    SendCommand(connection, Commands.ConnectionAccepted);
                    break;

                case ConnectionState.Connecting:
                    logger?.Error($"Server connections should not be in {nameof(ConnectionState.Connecting)} state");
                    break;
            }
        }


        private void RejectConnectionWithReason(IEndPoint endPoint, RejectReason reason)
        {
            SendCommandUnconnected(endPoint, Commands.ConnectionRejected, (byte)reason);
        }


        void HandleConnectionAccepted(Connection connection)
        {
            switch (connection.State)
            {
                case ConnectionState.Created:
                    logger?.Error($"Accepted Connections should not be in {nameof(ConnectionState.Created)} state");
                    break;

                case ConnectionState.Connected:
                    // ignore this, command may have been re-sent or Received twice
                    break;

                case ConnectionState.Connecting:
                    connection.State = ConnectionState.Connected;
                    OnConnected?.Invoke(connection);
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
                    logger?.Error($"Rejected Connections should not be in {nameof(ConnectionState.Created)} state");
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
                // if reason is ByLocal, then change it to ByRemote for sending
                byte byteReason = (byte)(reason == DisconnectReason.RequestedByLocalPeer
                    ? DisconnectReason.RequestedByRemotePeer
                    : reason);
                SendCommand(connection, Commands.Disconnect, byteReason);
            }

            // tell high level
            OnDisconnected?.Invoke(connection, reason);
        }

        internal void FailedToConnect(Connection connection, RejectReason reason)
        {
            if (logger.Enabled(LogType.Warning)) logger.Log(LogType.Warning, $"Connection Failed to connect: {reason}");

            RemoveConnection(connection);

            // tell high level
            OnConnectionFailed?.Invoke(connection, reason);
        }

        internal void RemoveConnection(Connection connection)
        {
            // shouldn't be trying to removed a destroyed connected
            logger?.Assert(connection.State != ConnectionState.Destroyed && connection.State != ConnectionState.Removing);

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

                // was closed while in conn.Update
                // dont continue loop,
                if (!active) { return; }
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
                if (!removed)
                {
                    logger?.Error($"Failed to remove {connection} from connection set");
                }
            }
            connectionsToRemove.Clear();
        }
    }
}
