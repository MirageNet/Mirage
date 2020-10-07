using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace Mirror
{
    public class MultiplexTransport : Transport
    {

        public Transport[] transports;

        private Channel<IConnection> accepted;
        private int stillAccepting = 0;

        public override IEnumerable<string> Scheme =>
            transports
                .Where(transport => transport.Supported)
                .SelectMany(transport => transport.Scheme);

        private Transport GetTransport()
        {
            foreach (Transport transport in transports)
            {
                if (transport.Supported)
                    return transport;
            }
            throw new PlatformNotSupportedException("None of the transports is supported in this platform");
        }

        public override bool Supported => GetTransport() != null;

        public override UniTask<IConnection> AcceptAsync()
        {
            if (accepted == null)
            {
                accepted = Channel.CreateSingleConsumerUnbounded<IConnection>();
                stillAccepting = transports.Length;
                foreach (Transport transport in transports)
                {
                    _ = AcceptLoopAsync(transport);
                }
            }

            if (stillAccepting == 0)
                return UniTask.FromResult<IConnection>(null);

            return accepted.Reader.ReadAsync();
        }

        private async UniTask AcceptLoopAsync(Transport transport)
        {
            try
            {
                IConnection connection = await transport.AcceptAsync();

                while (connection != null)
                {
                    accepted.Writer.TryWrite(connection);
                    connection = await transport.AcceptAsync();
                }
            }
            finally
            {
                stillAccepting--;

                if (stillAccepting == 0)
                {
                    // if last one out the room,  turn off the lights
                    // there are no more pending accepts
                    accepted.Writer.TryWrite(null);
                }
            }
        }

        public override  UniTask<IConnection> ConnectAsync(Uri uri)
        {
            foreach (Transport transport in transports)
            {
                if (transport.Supported && transport.Scheme.Contains(uri.Scheme))
                    return transport.ConnectAsync(uri);
            }
            throw new ArgumentException($"No transport was able to connect to {uri}");
        }

        public override void Disconnect()
        {
            foreach (Transport transport in transports)
                transport.Disconnect();
        }

        public override async UniTask ListenAsync()
        {
            IEnumerable<UniTask> tasks = from t in transports select t.ListenAsync();
            await UniTask.WhenAll(tasks);
            accepted = null;
        }

        public override IEnumerable<Uri> ServerUri() =>
            transports
                .Where(transport => transport.Supported)
                .SelectMany(transport => transport.ServerUri());
    }
}