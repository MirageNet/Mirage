
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

namespace Mirror.Udp
{
    public class UdpTransport : Transport
    {
        UdpClient client;
        public int Port = 7777;

        public override IEnumerable<string> Scheme => new[] { "udp4" };

        public override bool Supported => Application.platform != RuntimePlatform.WebGLPlayer;

        //Server starting to listen for new connections
        public override Task ListenAsync()
        {
            client = new UdpClient(new IPEndPoint(IPAddress.Any, Port));
            return Task.CompletedTask;
        }

        public override void Disconnect()
        {
            client.Client?.Disconnect(false);
        }

        //Client connecting to remote Server
        public override async Task<IConnection> ConnectAsync(Uri uri)
        {
            int port = uri.IsDefaultPort ? Port : uri.Port;
            client = new UdpClient(new IPEndPoint(IPAddress.Parse(uri.Host), port));
            await Task.CompletedTask;
            return new UdpConnection(client.Client);
        }

        //Server accepting a new connection
        public override async Task<IConnection> AcceptAsync()
        {
            try
            {
                Socket clientConnection = new Socket(SocketType.Dgram, ProtocolType.Udp);
                UdpReceiveResult receivedResult = await client.ReceiveAsync();
                clientConnection.Bind(receivedResult.RemoteEndPoint);
                await Task.CompletedTask;
                return new UdpConnection(clientConnection);
            }
            catch (ObjectDisposedException)
            {
                // expected,  the connection was closed
                return null;
            }
        }

        public override IEnumerable<Uri> ServerUri()
        {
            var builder = new UriBuilder
            {
                Host = Dns.GetHostName(),
                Port = this.Port,
                Scheme = "udp4"
            };

            return new[] { builder.Uri };
        }

        public void OnApplicationQuit()
        {
            if (client.Client.Connected)
            {
                client.Client?.Disconnect(false);
            }
        }
    }
}
