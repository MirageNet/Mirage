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

    /// <summary>
    /// Controls flow of data in/out of mirage, Uses <see cref="ISocket"/>
    /// </summary>
    public sealed class Peer
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

        // todo SendUnreliable
        // todo SendNotify

        readonly ISocket socket;
        readonly IDataHandler dataHandler;
        readonly Config config;
        readonly Time time;

        readonly ConnectKeyValidator connectKeyValidator;

        readonly Dictionary<EndPoint, Connection> connections;

        readonly byte[] commandBuffer = new byte[3];

        public event Action<IConnection> OnConnected;
        public event Action<IConnection, DisconnectReason> OnDisconnected;
        public event Action<IConnection, RejectReason> OnConnectionFailed;

        public Peer(ISocket socket, IDataHandler dataHandler, Config config, ILogger logger)
        {
            this.logger = logger ?? Debug.unityLogger;
            this.socket = socket ?? throw new ArgumentNullException(nameof(socket));
            this.dataHandler = dataHandler ?? throw new ArgumentNullException(nameof(dataHandler));
            this.config = config;
            time = new Time();

            connectKeyValidator = new ConnectKeyValidator();
        }


        public void Bind(EndPoint endPoint) => socket.Bind(endPoint);
        public IConnection Connect(EndPoint endPoint)
        {
            Connection connection = CreateNewConnection(endPoint);
            connection.State = ConnectionState.Connecting;

            // update now to send connectRequest command
            connection.Update();

            return connection;
        }

        public void Close()
        {
            socket.Close();
            // todo clean up other state
            throw new NotImplementedException();
        }

        internal void SendNotify(Connection connection) => throw new NotImplementedException();
        internal void SendReliable(Connection connection) => throw new NotImplementedException();
        internal void SendUnreliable(Connection connection, ArraySegment<byte> message)
        {
            // copy message to buffer 
            byte[] buffer = getBuffer();
            Buffer.BlockCopy(message.Array, message.Offset, buffer, 1, message.Count);
            // set header
            buffer[0] = (byte)PacketType.Unreliable;

            Send(connection, buffer, message.Count);
        }

        private void Send(Connection connection, Packet packet) => Send(connection, packet.data, packet.length);
        private void Send(Connection connection, byte[] data, int? length = null)
        {
            socket.Send(connection.EndPoint, data, length);
            connection.SetSendTime();
        }
        private void SendUnconnected(EndPoint endPoint, Packet packet) => SendUnconnected(endPoint, packet.data, packet.length);
        internal void SendUnconnected(EndPoint endPoint, byte[] data, int? length = null)
        {
            socket.Send(endPoint, data, length);
        }

        internal void SendCommandUnconnected(EndPoint endPoint, Commands command, byte? extra = null)
        {
            Packet packet = CreateCommandPacket(command, extra);
            SendUnconnected(endPoint, packet);
        }

        internal void SendCommand(Connection connection, Commands command, byte? extra = null)
        {
            Packet packet = CreateCommandPacket(command, extra);
            Send(connection, packet);
        }
        private Packet CreateCommandPacket(Commands command, byte? extra = null)
        {
            commandBuffer[0] = (byte)PacketType.Command;
            commandBuffer[1] = (byte)command;

            if (extra.HasValue)
            {
                commandBuffer[2] = extra.Value;
                return new Packet(commandBuffer, 3);
            }
            else
            {
                return new Packet(commandBuffer, 2);
            }
        }

        internal void SendKeepAlive(Connection connection)
        {
            commandBuffer[0] = (byte)PacketType.KeepAlive;
            Send(connection, commandBuffer, 1);
        }

        public void Update()
        {
            ReceiveLoop();
            UpdateConnections();
        }

        private void ReceiveLoop()
        {
            byte[] buffer = getBuffer();
            while (socket.Poll())
            {
                //todo do we need to pass in endpoint?
                EndPoint endPoint = null;
                socket.Recieve(buffer, ref endPoint, out int length);

                var packet = new Packet(buffer, length);

                if (!packet.IsValidSize())
                {
                    // handle message that are too small
                    throw new NotImplementedException();
                }


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

        private byte[] getBuffer()
        {
            // todo pool buffer
            // todo use MTU
            return new byte[1200];
        }


        private void HandleMessage(Connection connection, Packet packet)
        {
            switch (packet.type)
            {
                case PacketType.Command:
                    HandleCommand(connection, packet);
                    break;
                case PacketType.Unreliable:
                case PacketType.Notify:
                    // todo are these handled differently?
                    connection.ReceivePacket(packet);
                    break;
                case PacketType.KeepAlive:
                    // do nothing
                    break;
                default:
                    // handle message invalid packet type
                    throw new NotImplementedException();
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
                    // handle message invalid command type
                    throw new NotImplementedException();
            }
        }


        private void HandleNewConnection(EndPoint endPoint, Packet packet)
        {
            // if invalid, then reject without reason
            if (Validate(endPoint, packet)) { return; }

            if (AtMaxConnections())
            {
                RejectConnectionWithReason(endPoint, RejectReason.ServerFull);
            }
            else
            {
                AcceptNewConnection(endPoint);
            }
        }

        private bool Validate(EndPoint endPoint, Packet packet)
        {
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
            SendCommandUnconnected(endPoint, Commands.ConnectionAccepted, (byte)reason);
        }


        void HandleConnectionAccepted(Connection connection)
        {
            switch (connection.State)
            {
                case ConnectionState.Created:
                    Error($"Accepted Connections should not be in {nameof(ConnectionState.Created)} state");
                    break;

                case ConnectionState.Connected:
                    // ignore this, command may have been re-sent or recieved twice
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
                    var reason = (RejectReason)packet.data[2];
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
            var reason = (DisconnectReason)packet.data[2];
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
