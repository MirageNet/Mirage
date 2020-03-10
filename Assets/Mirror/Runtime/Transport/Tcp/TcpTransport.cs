// wraps Telepathy for use as HLAPI TransportLayer
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using UnityEngine;
using UnityEngine.Serialization;

namespace Mirror.Tcp
{
    public class TcpTransport : Transport
    {
        // scheme used by this transport
        // "tcp4" means tcp with 4 bytes header, network byte order
        private const string SCHEME = "tcp4";

        private readonly Client client = new Client();
        private readonly Server server = new Server();

        [FormerlySerializedAs("port")]
        public int Port = 7777;

        [Tooltip("Nagle Algorithm can be disabled by enabling NoDelay")]
        public bool NoDelay = true;

        public void Awake()
        {
            // dispatch the events from the server
            server.Connected += (connectionId) => OnServerConnected.Invoke(connectionId);
            server.Disconnected += (connectionId) => OnServerDisconnected.Invoke(connectionId);
            server.ReceivedData += (connectionId, data) => OnServerDataReceived.Invoke(connectionId, new ArraySegment<byte>(data), Channels.DefaultReliable);
            server.ReceivedError += (connectionId, error) => OnServerError.Invoke(connectionId, error);

            // dispatch events from the client
            client.Disconnected += () => OnClientDisconnected.Invoke();
            client.ReceivedData += (data) => OnClientDataReceived.Invoke(new ArraySegment<byte>(data), Channels.DefaultReliable);
            client.ReceivedError += (error) => OnClientError.Invoke(error);

            // configure
            client.NoDelay = NoDelay;
            server.NoDelay = NoDelay;

            Debug.Log("Tcp transport initialized!");
        }

        // client
        public override bool ClientConnected() { return client.Connected; }

        public override bool ClientSend(int channelId, ArraySegment<byte> segment)
        {
            _ = client.SendAsync(segment);
            return true;
        }
        public override void ClientDisconnect()
        {
            client.Disconnect();
        }

        // server
        public override bool ServerActive() { return server.IsActive; }
        public override void ServerStart()
        {
            _ = server.ListenAsync(Port);
        }

        public override bool ServerSend(List<int> connectionIds, int channelId, ArraySegment<byte> segment)
        {
            foreach (int connectionId in connectionIds)
                Server.Send(connectionId, segment);

            return true;
        }

        public override void ServerDisconnect(int connectionId)
        {
            server.Disconnect(connectionId);
        }

        public override string ServerGetClientAddress(int connectionId)
        {
            return server.GetClientAddress(connectionId);
        }

        public override void ServerStop() { server.Stop(); }

        public void LateUpdate()
        {
            server.Flush();
        }

        // common
        public override void Shutdown()
        {
            client.Disconnect();
            server.Stop();
        }

        public override int GetMaxPacketSize(int channelId)
        {
            // Telepathy's limit is Array.Length, which is int
            return int.MaxValue;
        }

        public override bool Available()
        {
            // C#'s built in TCP sockets run everywhere except on WebGL
            return Application.platform != RuntimePlatform.WebGLPlayer;
        }


        public override string ToString()
        {
            if (client.Connecting || client.Connected)
                return client.ToString();

            return server.IsActive ? server.ToString() : "";
        }

        public override Uri ServerUri()
        {
            UriBuilder builder = new UriBuilder();
            builder.Scheme = SCHEME;
            builder.Host = Dns.GetHostName();
            builder.Port = Port;

            return builder.Uri;
        }

        public override Task ClientConnectAsync(string address)
        {
            return client.ConnectAsync(address, Port);
        }

        public override Task ClientConnectAsync(Uri uri)
        {
            if (uri.Scheme != SCHEME)
                throw new ArgumentException($"Invalid url {uri}, use {SCHEME}://host:port instead", nameof(uri));

            int serverPort = uri.IsDefaultPort ? Port : uri.Port;

            return client.ConnectAsync(uri.Host, serverPort);
        }
    }
}
