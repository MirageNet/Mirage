using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

namespace Mirage.SocketLayer
{
    /// <summary>
    /// Handlers data from SocketLayer
    /// </summary>
    public interface IDataHandler
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection">player attached to connect that recieved the message, could be null</param>
        /// <param name="segment"></param>
        void ReceiveData(IConnection connection, ArraySegment<byte> segment);
    }

    internal class Time
    {
        public float Now => UnityEngine.Time.time;
    }
    internal class ConnectKeyValidator
    {
        // todo pass in key instead of having constant
        readonly byte[] key = new[] { (byte)'H' };

        public bool Validate(Packet packet)
        {
            byte keyByte = packet.data[2];

            return keyByte == key[0];
        }
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
        readonly ILogger logger;

        // todo SendUnreliable
        // tood SendNotify

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
        internal void SendUnreliable(Connection connection) => throw new NotImplementedException();

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
            throw new NotImplementedException();
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
                    // todo use better Exception type
                    throw new Exception($"Server connections should not be in {nameof(ConnectionState.Connecting)} state");
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
                    // todo use better Exception type
                    throw new Exception($"Accepted Connections should not be in {nameof(ConnectionState.Created)} state");

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
                    // todo use better Exception type
                    throw new Exception($"Rejected Connections should not be in {nameof(ConnectionState.Created)} state");
            }
        }

        internal void OnConnectionDisconnected(Connection connection, DisconnectReason reason)
        {
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
    internal struct Packet
    {
        const int MinPacketSize = 1;
        const int MinCommandSize = 2;
        /// <summary>
        /// Min size of message given to Mirage
        /// </summary>
        const int MinMessageSize = 3;

        public byte[] data;
        public int length;

        public Packet(byte[] data, int length)
        {
            this.data = data ?? throw new ArgumentNullException(nameof(data));
            this.length = length;
        }

        public bool IsValidSize()
        {
            if (length < MinPacketSize)
                return false;

            switch (type)
            {
                case PacketType.Command:
                    return length >= MinCommandSize;

                case PacketType.Unreliable:
                case PacketType.Notify:
                    return length >= MinMessageSize;

                default:
                case PacketType.KeepAlive:
                    return true;
            }
        }

        public PacketType type => (PacketType)data[0];
        public Commands command => (Commands)data[1];

        public ArraySegment<byte> ToSegment()
        {
            // ingore packet type
            return new ArraySegment<byte>(data, 1, length);
        }
    }

    internal enum PacketType
    {
        /// <summary>
        /// see <see cref="Commands"/>
        /// </summary>
        Command = 1,

        Unreliable = 2,
        Notify = 3,

        /// <summary>
        /// Used to keep connection alive.
        /// <para>Similar to ping/pong</para>
        /// </summary>
        KeepAlive = 4,
    }

    /// <summary>
    /// Small message used to control a connection
    /// <para>
    ///     <see cref="PacketType"/> and Commands uses their own byte/enum to split up the flow and add struture to the code.
    /// </para>
    /// </summary>
    internal enum Commands
    {
        /// <summary>
        /// Sent from client to request to connect to server
        /// </summary>
        ConnectRequest = 1,

        /// <summary>
        /// Sent when Server accepts client
        /// </summary>
        ConnectionAccepted = 2,

        /// <summary>
        /// Sent when server rejects client
        /// </summary>
        ConnectionRejected = 3,

        /// <summary>
        /// Sent from client or server to close connection
        /// </summary>
        Disconnect = 4,
    }

    /// <summary>
    /// Reson for reject sent from server
    /// </summary>
    public enum RejectReason
    {
        None = 0,
        ServerFull = 1,
        Timeout = 2,
    }

    public enum DisconnectReason
    {
        None,
        /// <summary>
        /// No message recieved in timeout window
        /// </summary>
        Timeout = 1,
        /// <summary>
        /// Disconnect called by higher level
        /// </summary>
        RequestedByPeer = 2,
    }

    // todo how should we use this?
    public sealed class PeerDebug
    {
        public int ReceivedBytes { get; set; }
        public int SentBytes { get; set; }
    }


    /// <summary>
    /// Creates <see cref="ISocket"/>
    /// </summary>
    /// <remarks>
    /// The only job of Transport is to create a <see cref="ISocket"/> that will be used by mirage to send/recieve data.
    /// <para>This is a MonoBehaviour so can be attached in the inspector</para>
    /// </remarks>
    // todo rename this to Transport when finished
    public abstract class TransportV2 : MonoBehaviour
    {
        public abstract ISocket CreateClientSocket();
        public abstract ISocket CreateServerSocket();

        public abstract EndPoint GetBindEndPoint();
        public abstract EndPoint GetConnectEndPoint(string address);

        public abstract bool ClientSupported { get; }
        public abstract bool ServerSupported { get; }
    }
}
