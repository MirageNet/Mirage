using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Mirage.KCP
{
    public class KcpTransport : Transport
    {
        Socket socket;

        [Header("Transport Configuration")]
        public ushort Port = 7777;

        [Range(15, 20)]
        [Tooltip("Used for DoS prevention,  clients must mine a HashCash with these many bits in order to connect, higher means more secure, but slower for the clients")]
        public int HashCashBits = 18;

        [Tooltip("How many messages can be sent simultaneously")]
        public int SendWindowSize = 32;
        [Tooltip("How many messages can be received")]
        public int ReceiveWindowSize = 8192;

        public KcpDelayMode delayMode = KcpDelayMode.Fast3;

        internal readonly Dictionary<IPEndPoint, KcpConnection> connections = new Dictionary<IPEndPoint, KcpConnection>(new IPEndpointComparer());

        public override IEnumerable<string> Scheme => new[] { "kcp" };

        readonly byte[] buffer = new byte[1500];

        public long ReceivedMessageCount { get; private set; }

        private long receivedBytes;
        private long sentBytes;

        #region server

        private UniTaskCompletionSource ListenCompletionSource;

        /// <summary>
        ///     Open up the port and listen for connections
        ///     Use in servers.
        /// </summary>
        /// <exception>If we cannot start the transport</exception>
        /// <returns></returns>
        public override UniTask ListenAsync()
        {
            try
            {
                socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp)
                {
                    DualMode = true
                };
                socket.Bind(new IPEndPoint(IPAddress.IPv6Any, Port));

                // transport started
                Started?.Invoke();

                ListenCompletionSource = new UniTaskCompletionSource();
                return ListenCompletionSource.Task;
            }
            catch (Exception ex)
            {
                return UniTask.FromException(ex);
            }
        }

        /// <summary>
        ///     Retrieves the address of this server.
        ///     Useful for network discovery
        /// </summary>
        /// <returns>the url at which this server can be reached</returns>
        public override IEnumerable<Uri> ServerUri()
        {
            var builder = new UriBuilder
            {
                Scheme = "kcp",
                Host = Dns.GetHostName(),
                Port = Port
            };
            return new[] { builder.Uri };
        }

        #endregion

        EndPoint ipv6EndPoint = new IPEndPoint(IPAddress.IPv6Any, 0);
        EndPoint ipv4EndPoint = new IPEndPoint(IPAddress.Any, 0);

        /// <summary>
        /// The higher level should call this method every tick to process
        /// the messages in this transport
        /// </summary>
        public override void Poll()
        {
            if (socket == null)
                return;

            EndPoint endPoint = socket.AddressFamily == AddressFamily.InterNetworkV6 ? ipv6EndPoint : ipv4EndPoint;

            try
            {
                while (socket.Poll(0, SelectMode.SelectRead))
                {
                    int msgLength = socket.ReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref endPoint);

                    ReceivedMessageCount++;
                    RawInput(endPoint, buffer, msgLength);
                }
            }
            catch (SocketException)
            {
                //Client disconnected
                Disconnect();
            }
        }

        void RawInput(EndPoint endpoint, byte[] data, int msgLength)
        {
            // is this a new connection?                    
            if (!connections.TryGetValue(endpoint as IPEndPoint, out KcpConnection connection))
            {
                // very first message from this endpoint.
                // lets validate it before we start KCP
                if (!Validate(data, msgLength))
                    return;

                int channel = KcpConnection.GetChannel(data);

                // handshake must start in reliable channel
                if (channel != Channel.Reliable)
                    return;

                receivedBytes += msgLength;
                // fire a separate task for handshake
                // that way if the client does not cooperate
                // we can continue with other clients
                ServerHandshake(endpoint, data, msgLength).Forget();
            }
            else
            {
                receivedBytes += msgLength;
                connection.HandlePacket(data, msgLength);
            }
        }

        private async UniTaskVoid ServerHandshake(EndPoint endpoint, byte[] data, int msgLength)
        {
            var connection = new KcpServerConnection(socket, endpoint, delayMode, SendWindowSize, ReceiveWindowSize);
            connections.Add(endpoint as IPEndPoint, connection);

            connection.Disconnected += () =>
            {
                connections.Remove(endpoint as IPEndPoint);
            };

            connection.DataSent += (length) =>
            {
                sentBytes += length;
            };

            connection.HandlePacket(data, msgLength);

            await connection.HandshakeAsync();

            // once handshake is completed,  then the connection has been accepted
            Connected?.Invoke(connection);
        }

        private readonly HashSet<HashCash> used = new HashSet<HashCash>();
        private readonly Queue<HashCash> expireQueue = new Queue<HashCash>();

        private bool Validate(byte[] data, int msgLength)
        {
            // do we have enough data in the buffer for a HashCash token?
            if (msgLength < Kcp.OVERHEAD + KcpConnection.RESERVED + HashCashEncoding.SIZE)
                return false;

            // read the token
            HashCash token = HashCashEncoding.Decode(data, Kcp.OVERHEAD + KcpConnection.RESERVED);

            RemoveExpiredTokens();

            // have this token been used?
            if (used.Contains(token))
                return false;

            // does the token validate?
            if (!token.Validate(Application.productName, HashCashBits))
                return false;

            used.Add(token);
            expireQueue.Enqueue(token);

            // brand new token, and it is for this app,  go on
            return true;
        }

        // remove all the tokens that expired
        private void RemoveExpiredTokens()
        {
            DateTime threshold = DateTime.UtcNow.AddMinutes(-10);

            while (expireQueue.Count > 0)
            {
                HashCash token = expireQueue.Peek();
                if (token.dt > threshold)
                    return;

                expireQueue.Dequeue();
                used.Remove(token);
            }
        }

        /// <summary>
        ///     Stop listening to the port
        /// </summary>
        public override void Disconnect()
        {
            socket?.Close();
            socket = null;
            ListenCompletionSource?.TrySetResult();
        }

        /// <summary>
        ///     Determines if this transport is supported in the current platform
        /// </summary>
        /// <returns>true if the transport works in this platform</returns>
        public override bool Supported => Application.platform != RuntimePlatform.WebGLPlayer;


        #region client
        /// <summary>
        ///     Connect to a server located at a provided uri
        /// </summary>
        /// <param name="uri">address of the server to connect to</param>
        /// <returns>The connection to the server</returns>
        /// <exception>If connection cannot be established</exception>
        public override UniTask<IConnection> ConnectAsync(Uri uri)
        {
            ushort port = (ushort)(uri.IsDefaultPort? Port : uri.Port);
            return ConnectAsync(uri.Host, port);
        }

        private async UniTask<IConnection> ConnectAsync(string host, ushort port)
        {
            IPAddress[] ipAddress = await Dns.GetHostAddressesAsync(host);
            if (ipAddress.Length < 1)
                throw new SocketException((int)SocketError.HostNotFound);

            var remoteEndpoint = new IPEndPoint(ipAddress[0], port);

            if (socket == null)
            {
                // initialize the socket if we don't have one
                socket = new Socket(remoteEndpoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            }

            var connection = new KcpClientConnection(socket, remoteEndpoint, delayMode, SendWindowSize, ReceiveWindowSize)
            {
            };

            connections.Add(remoteEndpoint, connection);

            connection.Disconnected += () =>
            {
                connections.Remove(remoteEndpoint);
            };

            connection.DataSent += (length) =>
            {
                sentBytes += length;
            };

            await connection.HandshakeAsync(HashCashBits);

            Connected?.Invoke(connection);
            return connection;
        }


        #endregion



        public override long ReceivedBytes => receivedBytes;

        public override long SentBytes => sentBytes;
    }
}
