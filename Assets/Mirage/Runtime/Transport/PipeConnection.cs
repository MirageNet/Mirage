using System;
using System.Net;
using Mirage.Logging;
using Mirage.SocketLayer;
using UnityEngine;

namespace Mirage
{
    /// <summary>
    /// A <see cref="SocketLayer.IConnection"/> that is directly sends data to a <see cref="IDataHandler"/>
    /// </summary>
    public class PipePeerConnection : SocketLayer.IConnection
    {
        static readonly ILogger logger = LogFactory.GetLogger<PipePeerConnection>();

        /// <summary>
        /// handler of other conection
        /// </summary>
        IDataHandler otherHandler;
        /// <summary>
        /// other connection that is passed to handler
        /// </summary>
        SocketLayer.IConnection otherConnection;

        /// <summary>
        /// Name used for debugging
        /// </summary>
        string name;

        private PipePeerConnection() { }

        public static (SocketLayer.IConnection clientConn, SocketLayer.IConnection serverConn) Create(IDataHandler clientHandler, IDataHandler serverHandler)
        {
            var client = new PipePeerConnection();
            var server = new PipePeerConnection();

            client.otherHandler = serverHandler ?? throw new ArgumentNullException(nameof(serverHandler));
            server.otherHandler = clientHandler ?? throw new ArgumentNullException(nameof(clientHandler));

            client.otherConnection = server;
            server.otherConnection = client;

            client.State = ConnectionState.Connected;
            server.State = ConnectionState.Connected;

            client.name = "[Client Pipe Connection]";
            server.name = "[Server Pipe Connection]";

            return (client, server);
        }

        public override string ToString()
        {
            return name;
        }

        EndPoint SocketLayer.IConnection.EndPoint => new PipeEndPoint();


        public ConnectionState State { get; private set; } = ConnectionState.Connected;

        void SocketLayer.IConnection.Disconnect()
        {
            State = ConnectionState.Disconnected;
        }

        INotifyToken SocketLayer.IConnection.SendNotify(byte[] packet)
        {
            logger.Assert(State == ConnectionState.Connected);
            otherHandler.ReceivePacket(otherConnection, new ArraySegment<byte>(packet));

            return new PipeNotifyToken();
        }

        void SocketLayer.IConnection.SendReliable(byte[] packet)
        {
            logger.Assert(State == ConnectionState.Connected);
            otherHandler.ReceivePacket(otherConnection, new ArraySegment<byte>(packet));
        }

        void SocketLayer.IConnection.SendUnreliable(byte[] packet)
        {
            logger.Assert(State == ConnectionState.Connected);
            otherHandler.ReceivePacket(otherConnection, new ArraySegment<byte>(packet));
        }


        public class PipeEndPoint : EndPoint
        {
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
