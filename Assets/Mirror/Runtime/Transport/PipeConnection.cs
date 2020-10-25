using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Mirror
{

    using UniTaskChannel = Cysharp.Threading.Tasks.Channel;

    /// <summary>
    /// A connection that is directly connected to another connection
    /// If you send data in one of them,  you receive it on the other one
    /// </summary>
    public class PipeConnection : IConnection
    {

        private PipeConnection connected;

        // should only be created by CreatePipe
        private PipeConnection()
        {

        }

        private static readonly Stack<MemoryStream> pool = new Stack<MemoryStream>();

        private readonly Channel<MemoryStream> incomingMsg = UniTaskChannel.CreateSingleConsumerUnbounded<MemoryStream>();

        public static (IConnection, IConnection) CreatePipe()
        {
            var c1 = new PipeConnection();
            var c2 = new PipeConnection();

            c1.connected = c2;
            c2.connected = c1;

            return (c1, c2);
        }

        public void Disconnect()
        {
            // disconnect both ends of the pipe
            connected.incomingMsg.Writer.TryComplete();
            incomingMsg.Writer.TryComplete();
        }

        // technically not an IPEndpoint,  will fix later
        public EndPoint GetEndPointAddress() => new IPEndPoint(IPAddress.Loopback, 0);
        
        public async UniTask<int> ReceiveAsync(MemoryStream buffer)
        {
            try
            {
                MemoryStream data = await incomingMsg.Reader.ReadAsync();
                data.Position = 0;
                buffer.SetLength(0);
                data.WriteTo(buffer);

                pool.Push(data);

                return 0;
            }
            catch (ChannelClosedException)
            {
                throw new EndOfStreamException();
            }
        }

        public UniTask SendAsync(ArraySegment<byte> data, int channel = Channel.Reliable)
        {
            MemoryStream stream = LeaseBuffer(data.Count);
            stream.Write(data.Array, data.Offset, data.Count);

            connected.incomingMsg.Writer.TryWrite(stream);
            return UniTask.CompletedTask;
        }

        private MemoryStream LeaseBuffer(int capacity)
        {
            if (pool.Count > 0)
            {
                MemoryStream stream = pool.Pop();
                stream.SetLength(0);
                return stream;
            }
            return new MemoryStream(capacity);
        }
    }
}
