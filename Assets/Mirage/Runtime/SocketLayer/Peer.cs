using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mirage.SocketLayer
{
    public interface ITime
    {
        double Now { get; }
    }
    internal class Time : ITime
    {
        public double Now => UnityEngine.Time.timeAsDouble;
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
        private readonly ILogger _logger;
        private readonly Metrics _metrics;
        private readonly ISocket _socket;
        private readonly IDataHandler _dataHandler;
        private readonly Config _config;
        private readonly int _maxPacketSize;
        private readonly Time _time;
        private readonly ConnectKeyValidator _connectKeyValidator;
        private readonly Pool<ByteBuffer> _bufferPool;
        private readonly Dictionary<IEndPoint, Connection> _connections = new Dictionary<IEndPoint, Connection>();

        // list so that remove can take place after foreach loops
        private readonly List<Connection> _connectionsToRemove = new List<Connection>();

        public event Action<IConnection> OnConnected;
        public event Action<IConnection, DisconnectReason> OnDisconnected;
        public event Action<IConnection, RejectReason> OnConnectionFailed;

        /// <summary>
        /// is server listening on or connected to endpoint
        /// </summary>
        private bool _active;
        public PoolMetrics PoolMetrics => _bufferPool.Metrics;

        public Peer(ISocket socket, int maxPacketSize, IDataHandler dataHandler, Config config = null, ILogger logger = null, Metrics metrics = null)
        {
            _logger = logger;
            _metrics = metrics;
            _config = config ?? new Config();
            _maxPacketSize = maxPacketSize;
            if (maxPacketSize < AckSystem.MIN_RELIABLE_HEADER_SIZE + 1)
                throw new ArgumentException($"Max packet size too small for AckSystem header", nameof(maxPacketSize));

            _socket = socket ?? throw new ArgumentNullException(nameof(socket));
            _dataHandler = dataHandler ?? throw new ArgumentNullException(nameof(dataHandler));
            _time = new Time();

            _connectKeyValidator = new ConnectKeyValidator(_config.key);

            _bufferPool = new Pool<ByteBuffer>(ByteBuffer.CreateNew, maxPacketSize, _config.BufferPoolStartSize, _config.BufferPoolMaxSize, _logger);
            Application.quitting += Application_quitting;
        }

        private void Application_quitting()
        {
            // make sure peer closes itself when applications closes.
            // this will make sure that disconnect Command is sent before applications closes
            if (_active)
                Close();
        }

        public void Bind(IEndPoint endPoint)
        {
            if (_active) throw new InvalidOperationException("Peer is already active");
            _active = true;
            _socket.Bind(endPoint);
        }

        public IConnection Connect(IEndPoint endPoint)
        {
            if (_active) throw new InvalidOperationException("Peer is already active");
            if (endPoint == null) throw new ArgumentNullException(nameof(endPoint));

            _active = true;
            _socket.Connect(endPoint);

            var connection = CreateNewConnection(endPoint);
            connection.State = ConnectionState.Connecting;

            // update now to send connectRequest command
            connection.Update();
            return connection;
        }

        public void Close()
        {
            if (!_active)
            {
                if (_logger.Enabled(LogType.Warning)) _logger.Log(LogType.Warning, "Peer is not active");
                return;
            }
            _active = false;
            Application.quitting -= Application_quitting;

            // send disconnect messages
            foreach (var conn in _connections.Values)
            {
                conn.Disconnect(DisconnectReason.RequestedByLocalPeer);
            }
            RemoveConnections();

            // close socket
            _socket.Close();
        }

        internal void Send(Connection connection, byte[] data, int length)
        {
            // connecting connections can send connect messages so is allowed
            // todo check connected before message are sent from high level
            _logger?.Assert(connection.State == ConnectionState.Connected || connection.State == ConnectionState.Connecting || connection.State == ConnectionState.Disconnected, connection.State);

            _socket.Send(connection.EndPoint, data, length);
            _metrics?.OnSend(length);
            connection.SetSendTime();

            if (_logger.Enabled(LogType.Log))
            {
                if ((PacketType)data[0] == PacketType.Command)
                {
                    _logger.Log($"Send to {connection} type: Command, {(Commands)data[1]}");
                }
                else
                {
                    _logger.Log($"Send to {connection} type: {(PacketType)data[0]}");
                }
            }
        }

        internal void SendCommandUnconnected(IEndPoint endPoint, Commands command, byte? extra = null)
        {
            using (var buffer = _bufferPool.Take())
            {
                var length = CreateCommandPacket(buffer, command, extra);

                _socket.Send(endPoint, buffer.array, length);
                _metrics?.OnSendUnconnected(length);
                if (_logger.Enabled(LogType.Log))
                {
                    _logger.Log($"Send to {endPoint} type: Command, {command}");
                }
            }
        }

        internal void SendConnectRequest(Connection connection)
        {
            using (var buffer = _bufferPool.Take())
            {
                var length = CreateCommandPacket(buffer, Commands.ConnectRequest, null);
                _connectKeyValidator.CopyTo(buffer.array);
                Send(connection, buffer.array, length + _connectKeyValidator.KeyLength);
            }
        }

        internal void SendCommand(Connection connection, Commands command, byte? extra = null)
        {
            using (var buffer = _bufferPool.Take())
            {
                var length = CreateCommandPacket(buffer, command, extra);
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
            using (var buffer = _bufferPool.Take())
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
            _metrics?.OnTick(_connections.Count);
        }


        private void ReceiveLoop()
        {
            using (var buffer = _bufferPool.Take())
            {
                // check active, because socket might have been closed by message handler
                while (_active && _socket.Poll())
                {
                    var length = _socket.Receive(buffer.array, out var receiveEndPoint);
                    if (length < 0)
                    {
                        _logger.Log(LogType.Warning, $"Receive returned less than 0 bytes, length={length}");
                        continue;
                    }

                    // this should never happen. buffer size is only MTU, if socket returns higher length then it has a bug.
                    if (length > _maxPacketSize)
                        throw new IndexOutOfRangeException($"Socket returned length above MTU. MaxPacketSize:{_maxPacketSize} length:{length}");

                    var packet = new Packet(buffer, length);

                    if (_connections.TryGetValue(receiveEndPoint, out var connection))
                    {
                        _metrics?.OnReceive(length);
                        HandleMessage(connection, packet);
                    }
                    else
                    {
                        _metrics?.OnReceiveUnconnected(length);
                        HandleNewConnection(receiveEndPoint, packet);
                    }
                }
            }
        }

        private void HandleMessage(Connection connection, Packet packet)
        {
            // ingore message of invalid size
            if (!connection.IsValidSize(packet))
            {
                if (_logger.Enabled(LogType.Log))
                {
                    _logger.Log($"Receive from {connection} was too small type:{packet.Type}, size:{packet.Length}");
                }
                return;
            }

            if (_logger.Enabled(LogType.Log))
            {
                if (packet.Type == PacketType.Command)
                {
                    _logger.Log($"Receive from {connection} type: Command, {packet.Command}");
                }
                else
                {
                    _logger.Log($"Receive from {connection} type: {packet.Type}");
                }
            }

            if (!connection.Connected)
            {
                // if not connected then we can only handle commands
                if (packet.Type == PacketType.Command)
                {
                    HandleCommand(connection, packet);
                    connection.SetReceiveTime();

                }
                else if (_logger.Enabled(LogType.Warning))
                {
                    _logger.Log(LogType.Warning, $"Receive from {connection} type: {packet.Type} while not connected");
                }

                // ignore other messages if not connected
                return;
            }

            // handle message when connected
            switch (packet.Type)
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
            switch (packet.Command)
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


            if (AtMaxConnections())
            {
                RejectConnectionWithReason(endPoint, RejectReason.ServerFull);
            }
            else if (!_connectKeyValidator.Validate(packet.Buffer.array, packet.Length))
            {
                RejectConnectionWithReason(endPoint, RejectReason.KeyInvalid);
            }
            // todo do other security stuff here:
            // - white/black list for endpoint?
            // (maybe a callback for developers to use?)
            else
            {
                AcceptNewConnection(endPoint);
            }
        }

        private bool Validate(Packet packet)
        {
            // key could be anything, so any message over 2 could be key.
            var minLength = 2;
            if (packet.Length < minLength)
                return false;

            if (packet.Type != PacketType.Command)
                return false;

            if (packet.Command != Commands.ConnectRequest)
                return false;

            return true;
        }

        private bool AtMaxConnections()
        {
            return _connections.Count >= _config.MaxConnections;
        }
        private void AcceptNewConnection(IEndPoint endPoint)
        {
            if (_logger.Enabled(LogType.Log)) _logger.Log($"Accepting new connection from:{endPoint}");

            var connection = CreateNewConnection(endPoint);

            HandleConnectionRequest(connection);
        }

        private Connection CreateNewConnection(IEndPoint newEndPoint)
        {
            // create copy of endpoint for this connection
            // this is so that we can re-use the endpoint (reduces alloc) for receive and not worry about changing internal data needed for each connection
            var endPoint = newEndPoint?.CreateCopy();

            Connection connection;
            if (_config.DisableReliableLayer)
            {
                connection = new NoReliableConnection(this, endPoint, _dataHandler, _config, _maxPacketSize, _time, _logger, _metrics);
            }
            else
            {
                connection = new ReliableConnection(this, endPoint, _dataHandler, _config, _maxPacketSize, _time, _bufferPool, _logger, _metrics);
            }


            connection.SetReceiveTime();
            _connections.Add(endPoint, connection);
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
                    _logger?.Error($"Server connections should not be in {nameof(ConnectionState.Connecting)} state");
                    break;
            }
        }


        private void RejectConnectionWithReason(IEndPoint endPoint, RejectReason reason)
        {
            SendCommandUnconnected(endPoint, Commands.ConnectionRejected, (byte)reason);
        }

        private void HandleConnectionAccepted(Connection connection)
        {
            switch (connection.State)
            {
                case ConnectionState.Created:
                    _logger?.Error($"Accepted Connections should not be in {nameof(ConnectionState.Created)} state");
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

        private void HandleConnectionRejected(Connection connection, Packet packet)
        {
            switch (connection.State)
            {
                case ConnectionState.Connecting:
                    var reason = (RejectReason)packet.Buffer.array[2];
                    FailedToConnect(connection, reason);
                    break;

                default:
                    _logger?.Error($"Rejected Connections should not be in {nameof(ConnectionState.Created)} state");
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
                var byteReason = (byte)(reason == DisconnectReason.RequestedByLocalPeer
                    ? DisconnectReason.RequestedByRemotePeer
                    : reason);
                SendCommand(connection, Commands.Disconnect, byteReason);
            }

            // tell high level
            OnDisconnected?.Invoke(connection, reason);
        }

        internal void FailedToConnect(Connection connection, RejectReason reason)
        {
            if (_logger.Enabled(LogType.Warning)) _logger.Log(LogType.Warning, $"Connection Failed to connect: {reason}");

            RemoveConnection(connection);

            // tell high level
            OnConnectionFailed?.Invoke(connection, reason);
        }

        internal void RemoveConnection(Connection connection)
        {
            // shouldn't be trying to removed a destroyed connected
            _logger?.Assert(connection.State != ConnectionState.Destroyed && connection.State != ConnectionState.Removing);

            connection.State = ConnectionState.Removing;
            _connectionsToRemove.Add(connection);
        }

        private void HandleConnectionDisconnect(Connection connection, Packet packet)
        {
            DisconnectReason reason;
            if (packet.Length == 3)
                reason = (DisconnectReason)packet.Buffer.array[2];
            else
                reason = DisconnectReason.None;

            connection.Disconnect(reason, false);
        }

        private void UpdateConnections()
        {
            foreach (var connection in _connections.Values)
            {
                connection.Update();

                // was closed while in conn.Update
                // dont continue loop,
                if (!_active) { return; }
            }

            RemoveConnections();
        }

        private void RemoveConnections()
        {
            if (_connectionsToRemove.Count == 0)
                return;

            foreach (var connection in _connectionsToRemove)
            {
                var removed = _connections.Remove(connection.EndPoint);
                connection.State = ConnectionState.Destroyed;

                if (connection is IDisposable disposable)
                    disposable.Dispose();

                // value should be removed from dictionary
                if (!removed)
                {
                    _logger?.Error($"Failed to remove {connection} from connection set");
                }
            }
            _connectionsToRemove.Clear();
        }
    }
}
