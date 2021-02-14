using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Mirror.TransportAdaptor
{
    public class ServerAdaptorConnection : AdaptorConnection
    {
        public ServerAdaptorConnection(int id, MirrorTransportAdaptor adaptor) : base(id, adaptor)
        {
        }

        public override void Disconnect() => transport.ServerDisconnect(id);

        public override EndPoint GetEndPointAddress() => throw new NotImplementedException();

        public override UniTask SendAsync(ArraySegment<byte> data, int channel = 0)
        {
            transport.ServerSend(id, channel, data);
            return UniTask.CompletedTask;
        }
    }
    public class ClientAdaptorConnection : AdaptorConnection
    {
        public ClientAdaptorConnection(int id, MirrorTransportAdaptor adaptor) : base(id, adaptor)
        {
        }

        public override void Disconnect() => transport.ClientDisconnect();

        public override EndPoint GetEndPointAddress() => throw new NotImplementedException();

        public override UniTask SendAsync(ArraySegment<byte> data, int channel = 0)
        {
            transport.ClientSend(channel, data);
            return UniTask.CompletedTask;
        }
    }
    public abstract class AdaptorConnection : IConnection
    {
        protected readonly MirrorTransportAdaptor adaptor;
        protected readonly int id;
        protected readonly MirrorTransport transport;
        private bool open;

        public AdaptorConnection(int id, MirrorTransportAdaptor adaptor)
        {
            this.adaptor = adaptor;
            transport = adaptor.Inner;
            this.id = id;

            open = true;
        }

        public abstract void Disconnect();
        public abstract EndPoint GetEndPointAddress();
        public abstract UniTask SendAsync(ArraySegment<byte> data, int channel = 0);

        Queue<(ArraySegment<byte> data, int channel)> dataQueue = new Queue<(ArraySegment<byte> data, int channel)>();
        private AutoResetUniTaskCompletionSource dataAvailable;

        public async UniTask<int> ReceiveAsync(MemoryStream buffer)
        {
            await WaitForMessages();

            ThrowIfClosed();

            (ArraySegment<byte> data, int channel) = dataQueue.Dequeue();

            buffer.SetLength(data.Count);
            buffer.Write(data.Array, data.Offset, data.Count);
            buffer.Position = data.Count;

            return channel;
        }

        private async UniTask WaitForMessages()
        {
            while (open && dataQueue.Count == 0)
            {
                dataAvailable = AutoResetUniTaskCompletionSource.Create();
                await dataAvailable.Task;
            }
        }
        private void ThrowIfClosed()
        {
            if (!open)
            {
                throw new EndOfStreamException();
            }
        }

        internal void MarkAsClosed()
        {
            open = false;
        }

        internal void OnData(ArraySegment<byte> data, int channel)
        {
            dataQueue.Enqueue((data, channel));
            dataAvailable.TrySetResult();
        }
    }
    public class MirrorTransportAdaptor : Transport
    {
        [SerializeField] MirrorTransport inner;
        [SerializeField] string scheme;

        private ClientAdaptorConnection clientConnection;
        private UniTaskCompletionSource listenCompletionSource;
        private Dictionary<int, ServerAdaptorConnection> serverConnections;

        internal MirrorTransport Inner => inner;

        public override IEnumerable<string> Scheme { get { yield return scheme; } }
        public override bool Supported => inner.Available();

        public override async UniTask<IConnection> ConnectAsync(Uri uri)
        {
            bool connected = false;
            bool disconnected = false;
            inner.OnClientConnected.AddListener(() =>
            {
                clientConnection = new ClientAdaptorConnection(default, this);
                connected = true;
            });
            inner.OnClientDataReceived.AddListener((data, channel) =>
            {
                clientConnection.OnData(data, channel);
            });
            inner.OnClientDisconnected.AddListener(() =>
            {
                clientConnection?.MarkAsClosed();
                // todo does this need to be called? call it just incase?
                clientConnection?.Disconnect();
                clientConnection = null;
                disconnected = true;
            });
            inner.OnClientError.AddListener((ex) =>
            {
                Debug.LogException(ex);
            });

            // todo is host name enough for this?
            inner.ClientConnect(uri.Host);

            while (!connected)
            {
                if (disconnected)
                {
                    throw new Exception("failed to connect");
                }
                await UniTask.Yield();
            }

            return clientConnection;
        }

        public override void Disconnect()
        {
            clientConnection?.Disconnect();
            clientConnection = null;
            listenCompletionSource?.TrySetResult();
        }

        public override UniTask ListenAsync()
        {
            inner.OnServerConnected.AddListener((id) =>
            {
                serverConnections.Add(id, new ServerAdaptorConnection(id, this));
            });
            inner.OnServerDataReceived.AddListener((id, data, channel) =>
            {
                if (serverConnections.TryGetValue(id, out ServerAdaptorConnection conn))
                {
                    conn.OnData(data, channel);
                }
                else
                {
                    Debug.LogError($"Can't find connection for {id}");
                }
            });
            inner.OnServerDisconnected.AddListener((id) =>
            {
                if (serverConnections.TryGetValue(id, out ServerAdaptorConnection conn))
                {
                    conn.MarkAsClosed();
                }
                serverConnections.Remove(id);
            });
            inner.OnServerError.AddListener((id, ex) =>
            {
                Debug.LogException(ex);
            });

            listenCompletionSource = new UniTaskCompletionSource();
            serverConnections = new Dictionary<int, ServerAdaptorConnection>();
            inner.ServerStart();
            return listenCompletionSource.Task;
        }

        public override IEnumerable<Uri> ServerUri()
        {
            yield return inner.ServerUri();
        }
    }
}
