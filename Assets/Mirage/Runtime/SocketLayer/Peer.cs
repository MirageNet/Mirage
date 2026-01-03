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
        public double Now => UnityEngine.Time.unscaledTimeAsDouble;
    }

    public interface IPeer
    {
        event Action<IConnection> OnConnected;
        event Action<IConnection, RejectReason> OnConnectionFailed;
        event Action<IConnection, DisconnectReason> OnDisconnected;

        void Bind(IBindEndPoint endPoint);
        IConnection Connect(IConnectEndPoint endPoint);
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
        internal readonly Pool<ByteBuffer> _bufferPool;
        private readonly List<Connection> _connections = new List<Connection>();
        /// <summary>lookup for stateless connections</summary>
        private readonly Dictionary<IConnectionHandle, Connection> _connectionLookup = new Dictionary<IConnectionHandle, Connection>();

        // list so that remove can take place after foreach loops
        private readonly List<Connection> _connectionsToRemove = new List<Connection>();

        public event Action<IConnection> OnConnected;
        public event Action<IConnection, DisconnectReason> OnDisconnected;
        public event Action<IConnection, RejectReason> OnConnectionFailed;

        /// <summary>
        /// is server listening on or connected to endPoint
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
            if (_config.MaxReliableFragments > 255)
                throw new ArgumentOutOfRangeException(nameof(_config.MaxReliableFragments), _config.MaxReliableFragments, "MaxReliableFragments must be less than or equal to 255");

            _socket = socket ?? throw new ArgumentNullException(nameof(socket));
            _dataHandler = dataHandler ?? throw new ArgumentNullException(nameof(dataHandler));
            _time = new Time();

            _connectKeyValidator = new ConnectKeyValidator(_config.key);

            _bufferPool = new Pool<ByteBuffer>(ByteBuffer.CreateNew, maxPacketSize, _config.BufferPoolStartSize, _config.BufferPoolMaxSize, _logger);
            socket.SetTickEvents(_maxPacketSize, OnDataEvent, OnDisconnectEvent);

            Application.quitting += Application_quitting;
        }

        private void Application_quitting()
        {
            // make sure peer closes itself when applications closes.
            // this will make sure that disconnect Command is sent before applications closes
            if (_active)
                Close();
        }

        public void Bind(IBindEndPoint endPoint)
        {
            if (_active) throw new InvalidOperationException("Peer is already active");
            _active = true;
            _socket.Bind(endPoint);
        }

        public IConnection Connect(IConnectEndPoint endPoint)
        {
            if (_active) throw new InvalidOperationException("Peer is already active");
            if (endPoint == null) throw new ArgumentNullException(nameof(endPoint));

            _active = true;
            var handle = _socket.Connect(endPoint);

            var connection = CreateNewConnection(handle);
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
            foreach (var conn in _connections)
            {
                conn.DisconnectInternal(DisconnectReason.RequestedByLocalPeer);
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

            _socket.Send(connection.Handle, data.AsSpan(0, length));
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

        internal void SendCommandUnconnected(IConnectionHandle handle, Commands command, byte? extra = null)
        {
            using (var buffer = _bufferPool.Take())
            {
                var length = CreateCommandPacket(buffer, command, extra);

                _socket.Send(handle, buffer.array.AsSpan(0, length));
                _metrics?.OnSendUnconnected(length);
                if (_logger.Enabled(LogType.Log))
                {
                    _logger.Log($"Send to {handle} type: Command, {command}");
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
        /// Call this at end of frame to send new batches
        /// </summary>
        public void UpdateSent()
        {
            UpdateConnections();
            _metrics?.OnTick(_connections.Count);
        }

        /// <summary>
        /// Call this at the start of the frame to receive new messages
        /// </summary>
        public void UpdateReceive()
        {
            try
            {
                // tick loop (push)
                _socket.Tick();

                // poll loop (pull)
                using (var buffer = _bufferPool.Take())
                {
                    // check active, because socket might have been closed by message handler
                    while (_active && _socket.Poll())
                    {
                        var length = _socket.Receive(buffer.array, out var handle);

                        if (length < 0 && _logger.Enabled(LogType.Warning))
                        {
                            _logger.Warn($"Receive returned less than 0 bytes, length={length}");
                            return;
                        }

                        // this should never happen. buffer size is only MTU, if socket returns higher length then it has a bug.
                        // NOTE: this can now happen if transport are pushing data to us (rather than pull)
                        if (length > _maxPacketSize)
                        {
                            _logger.Error($"Socket returned length above MTU. MaxPacketSize:{_maxPacketSize} length:{length}");
                            SafeDisconnectFromError(handle);
                            return;
                        }

                        OnData(handle, new Packet(buffer.array.AsSpan(0, length)));
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogException(e);
            }
        }

        /// <summary>
        /// Called if receiving invalid packet
        /// </summary>
        /// <param name="handle"></param>
        private void SafeDisconnectFromError(IConnectionHandle handle)
        {
            if (handle.IsStateful)
            {
                if (handle.SocketLayerConnection != null)
                {
                    // connected and stateful
                    var connection = (Connection)handle.SocketLayerConnection;
                    OnConnectionDisconnected(connection, DisconnectReason.InvalidPacket, true);
                }
                else
                {
                    // not connected and stateful
                    handle.Disconnect(null);
                }
            }
            else
            {
                if (_connectionLookup.TryGetValue(handle, out var connection))
                {
                    // connected and stateless
                    OnConnectionDisconnected(connection, DisconnectReason.InvalidPacket, true);
                }
                else
                {
                    // not connected and stateless
                    // just ignore
                }
            }
        }

        private void OnDataEvent(IConnectionHandle handle, ReadOnlySpan<byte> data)
        {
            var length = data.Length;
            if (length < 0 && _logger.Enabled(LogType.Warning))
            {
                _logger.Warn($"Receive returned less than 0 bytes, length={length}");
                return;
            }

            // this should never happen. buffer size is only MTU, if socket returns higher length then it has a bug.
            // NOTE: this can now happen if transport are pushing data to us (rather than pull)
            if (length > _maxPacketSize)
            {
                _logger.Error($"Socket returned length above MTU. MaxPacketSize:{_maxPacketSize} length:{length}");
                SafeDisconnectFromError(handle);
                return;
            }

            OnData(handle, new Packet(data));
        }
        private void OnDisconnectEvent(IConnectionHandle handle, ReadOnlySpan<byte> data, string reason)
        {
            _logger.Assert(handle.IsStateful, "only stateful connection should use OnDisconnect event");
            // todo assert connection is stateful
            // TODO handle data and reason better

            DisconnectReason disconnectReason;
            if (data.Length == 3)
                disconnectReason = (DisconnectReason)data[2];
            else
                disconnectReason = DisconnectReason.None;

            var connection = (Connection)handle.SocketLayerConnection;
            connection.DisconnectInternal(disconnectReason, false);
        }
        private void OnData(IConnectionHandle handle, Packet packet)
        {
            if (handle.IsStateful)
            {
                if (handle.SocketLayerConnection != null)
                {
                    var connection = (Connection)handle.SocketLayerConnection;
                    _metrics?.OnReceive(packet.Length);
                    HandleMessage(connection, packet);
                }
                else
                {
                    _metrics?.OnReceiveUnconnected(packet.Length);
                    HandleNewConnection(handle, packet);
                }
            }
            else
            {
                if (_connectionLookup.TryGetValue(handle, out var connection))
                {
                    _metrics?.OnReceive(packet.Length);
                    HandleMessage(connection, packet);
                }
                else
                {
                    _metrics?.OnReceiveUnconnected(packet.Length);
                    HandleNewConnection(handle, packet);
                }
            }
        }

        private void HandleMessage(Connection connection, Packet packet)
        {
            // ignore message of invalid size
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
                    _logger.Log($"Receive {packet.Length} bytes from {connection} type: Command, {packet.Command}");
                }
                else
                {
                    _logger.Log($"Receive {packet.Length} bytes from {connection} type: {packet.Type}");
                }
            }

            if (!connection.Connected)
            {
                // if not connected then we can only handle commands
                if (packet.Type == PacketType.Command)
                {
                    HandleCommand(connection, packet);
                    // only set time if valid packet
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

            // only set time if valid packet
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

        private void HandleNewConnection(IConnectionHandle handle, Packet packet)
        {
            // if invalid, then reject without reason
            if (!Validate(packet))
            {
                if (_config.SendRejectIfUnconnectedPacketIsInvalid)
                    RejectConnectionWithReason(handle, RejectReason.InvalidUnconnectedPacket);
                // else ignore
            }
            else if (AtMaxConnections())
            {
                if (_logger.Enabled(LogType.Warning))
                    _logger.Log(LogType.Warning, $"Reject Connection: At max connections");

                RejectConnectionWithReason(handle, RejectReason.ServerFull);
            }
            else if (!_connectKeyValidator.Validate(packet.Span))
            {
                if (_logger.Enabled(LogType.Warning))
                    _logger.Log(LogType.Warning, $"Reject Connection: Invalid key");

                RejectConnectionWithReason(handle, RejectReason.KeyInvalid);
            }
            // todo do other security stuff here:
            // - white/black list for endPoint?
            // (maybe a callback for developers to use?)
            else
            {
                AcceptNewConnection(handle);
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
        private void AcceptNewConnection(IConnectionHandle handle)
        {
            if (_logger.Enabled(LogType.Log)) _logger.Log($"Accepting new connection from:{handle}");

            var connection = CreateNewConnection(handle);

            HandleConnectionRequest(connection);
        }

        private Connection CreateNewConnection(IConnectionHandle newHandle)
        {
            IConnectionHandle handle;
            if (newHandle.IsStateful)
            {
                // dont need to create copy, because returned handle should not be reused by socket for stateful connections
                handle = newHandle;
            }
            else
            {
                // create copy of endPoint for this connection
                // this is so that we can re-use the endPoint (reduces alloc) for receive and not worry about changing internal data needed for each connection
                handle = newHandle?.CreateCopy();
            }


            Connection connection;
            if (_config.DisableReliableLayer)
                connection = new NoReliableConnection(this, handle, _dataHandler, _config, _maxPacketSize, _time, _logger, _metrics);
            else
                connection = new ReliableConnection(this, handle, _dataHandler, _config, _maxPacketSize, _time, _bufferPool, _logger, _metrics);

            connection.SetReceiveTime();

            if (handle.IsStateful)
                handle.SocketLayerConnection = connection;
            else
                _connectionLookup.Add(handle, connection);

            _connections.Add(connection);
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


        private void RejectConnectionWithReason(IConnectionHandle handle, RejectReason reason)
        {
            SendCommandUnconnected(handle, Commands.ConnectionRejected, (byte)reason);
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
                    var reason = (RejectReason)packet.Span[2];
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

            // tell stateful connections
            if (connection.Handle.IsStateful)
                connection.Handle.Disconnect(null);
        }

        internal void FailedToConnect(Connection connection, RejectReason reason)
        {
            if (_logger.Enabled(LogType.Warning)) _logger.Log(LogType.Warning, $"Connection Failed to connect: {reason}");

            RemoveConnection(connection);

            // tell high level
            OnConnectionFailed?.Invoke(connection, reason);

            // tell stateful connections
            if (connection.Handle.IsStateful)
                connection.Handle.Disconnect(null);
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
                reason = (DisconnectReason)packet.Span[2];
            else
                reason = DisconnectReason.None;

            connection.DisconnectInternal(reason, false);
        }

        private void UpdateConnections()
        {
            foreach (var connection in _connections)
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
                connection.State = ConnectionState.Destroyed;

                var removed = _connections.Remove(connection);
                if (!removed)
                {
                    // value should be removed from list
                    _logger?.Error($"Failed to remove {connection} from connection list");
                }

                if (!connection.Handle.IsStateful)
                {
                    // value should be removed from dictionary
                    var removedLookup = _connectionLookup.Remove(connection.Handle);
                    if (!removedLookup)
                    {
                        _logger?.Error($"Failed to remove {connection} from connection lookup");
                    }
                }


                // try/catch just incase something goes wrong
                // if it does we dont want to be stuck running dispose every frame
                try
                {
                    if (connection is IDisposable disposable)
                        disposable.Dispose();
                }
                catch (Exception ex)
                {
                    _logger?.Error($"Connection.Dispose threw: {ex}");
                }
            }

            _connectionsToRemove.Clear();
        }

        public int GetMaxUnreliableMessageSize()
        {
            // for both Reliable and NoReliable connections, unreliable is just a header + message
            // header is 1 byte for type, 2 for length
            const int header = 1 + Batch.MESSAGE_LENGTH_SIZE;
            return _maxPacketSize - header;
        }

        public int GetMaxNotifyMessageSize()
        {
            if (_config.DisableReliableLayer)
            {
                // NoReliableConnection calls SendReliable for notify
                const int header = 1 + Batch.MESSAGE_LENGTH_SIZE; // packet type + message length
                return _maxPacketSize - header;
            }
            else
            {
                // from AckSystem
                return _maxPacketSize - AckSystem.NOTIFY_HEADER_SIZE;
            }
        }

        public int GetMaxReliableMessageSize()
        {
            if (_config.DisableReliableLayer)
            {
                // from NoReliableConnection
                const int header = 1 + Batch.MESSAGE_LENGTH_SIZE; // packet type + message length
                return _maxPacketSize - header;
            }
            else
            {
                // from AckSystem
                // if fragmentation is enabled
                if (_config.MaxReliableFragments >= 0)
                {
                    var sizePerFragment = _maxPacketSize - AckSystem.MIN_RELIABLE_FRAGMENT_HEADER_SIZE;
                    return _config.MaxReliableFragments * sizePerFragment;
                }
                else
                {
                    // if not fragmented
                    return _maxPacketSize - AckSystem.MIN_RELIABLE_HEADER_SIZE;
                }
            }
        }
    }
}
