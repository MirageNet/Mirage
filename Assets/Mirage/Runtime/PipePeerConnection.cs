using System;
using Mirage.Logging;
using Mirage.SocketLayer;
using UnityEngine;

namespace Mirage
{
    /// <summary>
    /// A <see cref="IConnection"/> that is directly sends data to a <see cref="IDataHandler"/>
    /// </summary>
    public class PipePeerConnection : IConnection
    {
        private static readonly ILogger logger = LogFactory.GetLogger<PipePeerConnection>();

        /// <summary>
        /// handler of other connection
        /// </summary>
        private IDataHandler _otherHandler;

        /// <summary>
        /// other connection that is passed to handler
        /// </summary>
        private IConnection _otherConnection;

        /// <summary>
        /// Name used for debugging
        /// </summary>
        private string _name;
        private Action _onDisconnect;

        private PipePeerConnection() { }

        public static (IConnection clientConn, IConnection serverConn) Create(IDataHandler clientHandler, IDataHandler serverHandler, Action clientOnDisconnect, Action serverOnDisconnect)
        {
            var client = new PipePeerConnection();
            var server = new PipePeerConnection();

            client._onDisconnect = clientOnDisconnect;
            server._onDisconnect = serverOnDisconnect;

            client._otherHandler = serverHandler ?? throw new ArgumentNullException(nameof(serverHandler));
            server._otherHandler = clientHandler ?? throw new ArgumentNullException(nameof(clientHandler));

            client._otherConnection = server;
            server._otherConnection = client;

            client.State = ConnectionState.Connected;
            server.State = ConnectionState.Connected;

            client._name = "[Client Pipe Connection]";
            server._name = "[Server Pipe Connection]";

            return (client, server);
        }

        public override string ToString()
        {
            return _name;
        }

        IEndPoint IConnection.EndPoint => new PipeEndPoint();
        void IConnection.FlushBatch() { }

        public ConnectionState State { get; private set; } = ConnectionState.Connected;

        void IConnection.Disconnect()
        {
            if (State == ConnectionState.Disconnected)
                return;

            State = ConnectionState.Disconnected;
            _onDisconnect?.Invoke();

            // tell other connection to also disconnect
            _otherConnection.Disconnect();
        }

        public INotifyToken SendNotify(byte[] packet, int offset, int length)
        {
            if (State == ConnectionState.Disconnected)
                return default;

            receive(packet, offset, length);

            return new PipeNotifyToken();
        }
        public INotifyToken SendNotify(ArraySegment<byte> packet) => SendNotify(packet.Array, packet.Offset, packet.Count);
        public INotifyToken SendNotify(byte[] packet) => SendNotify(packet, 0, packet.Length);

        public void SendNotify(byte[] packet, int offset, int length, INotifyCallBack callBacks)
        {
            if (State == ConnectionState.Disconnected)
                return;

            receive(packet, offset, length);

            callBacks.OnDelivered();
        }
        public void SendNotify(ArraySegment<byte> packet, INotifyCallBack callBacks) => SendNotify(packet.Array, packet.Offset, packet.Count, callBacks);
        public void SendNotify(byte[] packet, INotifyCallBack callBacks) => SendNotify(packet, 0, packet.Length, callBacks);


        public void SendReliable(byte[] message, int offset, int length)
        {
            if (State == ConnectionState.Disconnected)
                return;

            receive(message, offset, length);
        }
        public void SendReliable(ArraySegment<byte> packet) => SendReliable(packet.Array, packet.Offset, packet.Count);
        public void SendReliable(byte[] packet) => SendReliable(packet, 0, packet.Length);


        public void SendUnreliable(byte[] packet, int offset, int length)
        {
            if (State == ConnectionState.Disconnected)
                return;

            receive(packet, offset, length);
        }
        public void SendUnreliable(ArraySegment<byte> packet) => SendUnreliable(packet.Array, packet.Offset, packet.Count);
        public void SendUnreliable(byte[] packet) => SendUnreliable(packet, 0, packet.Length);

        private void receive(byte[] packet, int offset, int length)
        {
            logger.Assert(State == ConnectionState.Connected);
            _otherHandler.ReceiveMessage(_otherConnection, new ArraySegment<byte>(packet, offset, length));
        }

        public class PipeEndPoint : IEndPoint
        {
            IEndPoint IEndPoint.CreateCopy()
            {
                // never need copy of pipeendpoint
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Token that invokes <see cref="INotifyToken.Delivered"/> immediately
        /// </summary>
        public struct PipeNotifyToken : INotifyToken
        {
            public event Action Delivered
            {
                add
                {
                    value.Invoke();
                }
                remove
                {
                    // nothing
                }
            }
            public event Action Lost
            {
                add
                {
                    // nothing
                }
                remove
                {
                    // nothing
                }
            }
        }
    }
}
