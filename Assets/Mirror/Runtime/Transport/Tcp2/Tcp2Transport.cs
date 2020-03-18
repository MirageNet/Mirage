﻿
using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Mirror.Tcp2
{
    public class Tcp2Transport : Transport2
    {
        private TcpListener listener;
        public int Port { get; set; }

        public override Task ListenAsync()
        {
            listener = TcpListener.Create(Port);
            listener.Start();
            return Task.CompletedTask;
        }

        public override void Disconnect()
        {
            listener?.Stop();
        }

        public override async Task<IConnection> ConnectAsync(Uri uri)
        {
            string host = uri.Host;
            int port = uri.IsDefaultPort ? Port : uri.Port;

            var client = new TcpClient(AddressFamily.InterNetworkV6);
            // works with IPv6 and IPv4
            client.Client.DualMode = true;

            // NoDelay disables nagle algorithm. lowers CPU% and latency
            // but increases bandwidth
            client.NoDelay = true;
            client.LingerState = new LingerOption(true, 10);

            await client.ConnectAsync(host, port);

            return new TcpConnection(client);
        }

        public override async Task<IConnection> AcceptAsync()
        {
            TcpClient client = await listener.AcceptTcpClientAsync();
            return new TcpConnection(client);
        }
    }
}