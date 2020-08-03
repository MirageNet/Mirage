
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
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

        public override async Task<IConnection> AcceptAsync()
        {
            await Task.CompletedTask;
            return new UdpConnection(client);
        }

        public override async Task<IConnection> ConnectAsync(Uri uri)
        {
            IPAddress server_address = IPAddress.Parse(uri.Host);
            int port = uri.IsDefaultPort ? Port : uri.Port;
            IPEndPoint endpoint = new IPEndPoint(server_address, port);
            client.Connect(endpoint);

            await Task.CompletedTask;

            return new UdpConnection(client);
        }

        public override void Disconnect()
        {
            client.Close();
        }

        public override Task ListenAsync()
        {
            client = new UdpClient(new IPEndPoint(IPAddress.Any, Port));
            return Task.CompletedTask;
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
    }
}
