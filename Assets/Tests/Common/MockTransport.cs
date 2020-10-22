﻿using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Mirror.Tests
{

    public class MockTransport : Transport
    {
        public readonly Channel<IConnection> AcceptConnections = Cysharp.Threading.Tasks.Channel.CreateSingleConsumerUnbounded<IConnection>();

        public override UniTask<IConnection> AcceptAsync()
        {
            return AcceptConnections.Reader.ReadAsync();
        }

        public readonly Channel<IConnection> ConnectConnections = Cysharp.Threading.Tasks.Channel.CreateSingleConsumerUnbounded<IConnection>();

        public override IEnumerable<string> Scheme => new []{"kcp"};

        public override bool Supported => true;

        public override UniTask<IConnection> ConnectAsync(Uri uri)
        {
            return ConnectConnections.Reader.ReadAsync();
        }

        public override void Disconnect()
        {
            AcceptConnections.Writer.TryWrite(null);
        }

        public override UniTask ListenAsync()
        {
            return UniTask.CompletedTask;
        }

        public override IEnumerable<Uri> ServerUri()
        {
            return new[] { new Uri("kcp://localhost") };
        }
    }
}
