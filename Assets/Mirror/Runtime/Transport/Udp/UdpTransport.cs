
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
        UdpConnection listener;
        public int Port = 7777;

        public override IEnumerable<string> Scheme => new[] { "udp4" };

        public override bool Supported => Application.platform != RuntimePlatform.WebGLPlayer;

        //Server starting to listen for new connections
        public override Task ListenAsync()
        {
            UdpClient client = new UdpClient(Port);
            listener = new UdpConnection(client);
            return client.ReceiveAsync();
        }

        public override void Disconnect()
        {
            listener?.Stop();
        }

        //Client connecting to remote Server
        public override async Task<IConnection> ConnectAsync(Uri uri)
        {
            IPAddress server_address = IPAddress.Parse(uri.Host);
            int port = uri.IsDefaultPort ? Port : uri.Port;

            await Task.CompletedTask;
            return new UdpConnection(new UdpClient(new IPEndPoint(server_address, port)));
        }

        //Server accepting a new connection
        public override async Task<IConnection> AcceptAsync()
        {
            await listener.client.Client.AcceptAsync();
            return listener;
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
            listener?.Stop();
        }
    }
}
