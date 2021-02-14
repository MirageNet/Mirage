using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Mirror.TransportAdaptor
{
    public class AdaptorConnection : IConnection
    {
        public void Disconnect()
        {
            throw new NotImplementedException();
        }

        public EndPoint GetEndPointAddress()
        {
            throw new NotImplementedException();
        }

        public UniTask<int> ReceiveAsync(MemoryStream buffer)
        {
            throw new NotImplementedException();
        }

        public UniTask SendAsync(ArraySegment<byte> data, int channel = 0)
        {
            throw new NotImplementedException();
        }
    }
    public class MirrorTransportAdaptor : Transport
    {
        [SerializeField] MirrorTransport inner;
        [SerializeField] string scheme;

        private AdaptorConnection clientConnection;

        public override IEnumerable<string> Scheme { get { yield return scheme; } }
        public override bool Supported => inner.Available();

        public override async UniTask<IConnection> ConnectAsync(Uri uri)
        {
            bool connected = false;
            inner.OnClientConnected.AddListener(() =>
            {
                connected = true;
            });

            // todo is host name enough for this?
            inner.ClientConnect(uri.Host);

            while (!connected)
            {
                await UniTask.Yield();
            }

            clientConnection = new AdaptorConnection();
            return clientConnection;
        }

        public override void Disconnect()
        {
            clientConnection?.Disconnect();
        }

        public override UniTask ListenAsync()
        {
            inner.ServerStart();
        }

        public override IEnumerable<Uri> ServerUri()
        {
            throw new NotImplementedException();
        }
    }
}
