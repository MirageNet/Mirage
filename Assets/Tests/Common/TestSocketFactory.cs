using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Mirage.SocketLayer;
using NSubstitute;

namespace Mirage.Tests
{
    /// <summary>
    /// Socket that can send message to other sockets
    /// </summary>
    public class TestSocket : ISocket
    {
        /// <summary>
        /// this static dictionary will act as the internet
        /// </summary>
        static Dictionary<EndPoint, TestSocket> allSockets = new Dictionary<EndPoint, TestSocket>();

        public static bool EndpointInUse(EndPoint endPoint) => allSockets.ContainsKey(endPoint);

        /// <summary>
        /// adds this socket as an option to receive data
        /// </summary>
        private void AddThisSocket()
        {
            if (allSockets.TryGetValue(endPoint, out TestSocket value))
            {
                if (value != this)
                {
                    throw new Exception($"endpoint [{endPoint}] already exist in socket dictionary with different value");
                }
            }
            else
            {
                allSockets[endPoint] = this;
            }
        }


        public readonly EndPoint endPoint;

        readonly Queue<Packet> received = new Queue<Packet>();
        public List<Packet> Sent = new List<Packet>();

        public TestSocket(EndPoint endPoint = null)
        {
            this.endPoint = endPoint ?? Substitute.For<EndPoint>();
        }


        void ISocket.Bind(EndPoint endPoint)
        {
            AddThisSocket();
        }

        //void ISocket.Connect(EndPoint endPoint)
        //{
        //    AddThisSocket();
        //}

        void ISocket.Close()
        {
            allSockets.Remove(endPoint);
        }

        bool ISocket.Poll()
        {
            return received.Count > 0;
        }

        int ISocket.Receive(byte[] data, ref EndPoint endPoint)
        {
            Packet next = received.Dequeue();
            endPoint = next.endPoint;
            int length = next.length;

            Buffer.BlockCopy(next.data, 0, data, 0, length);
            return length;
        }

        void ISocket.Send(EndPoint remoteEndPoint, byte[] data, int length)
        {
            AddThisSocket();

            if (!allSockets.TryGetValue(remoteEndPoint, out TestSocket other))
            {
                // other socket might have been closed
                return;
            }

            // create copy because data is from buffer
            byte[] clone = data.ToArray();
            Sent.Add(new Packet
            {
                endPoint = remoteEndPoint,
                data = clone,
                length = length
            });
            other.received.Enqueue(new Packet
            {
                endPoint = endPoint,
                data = clone,
                length = length
            });
        }

        public struct Packet
        {
            public EndPoint endPoint;
            public byte[] data;
            public int length;
        }
    }


    public class TestSocketFactory : SocketFactory
    {
        EndPoint serverEndpoint = Substitute.For<EndPoint>();

        public override ISocket CreateClientSocket()
        {
            return new TestSocket();
        }

        public override ISocket CreateServerSocket()
        {
            if (TestSocket.EndpointInUse(serverEndpoint))
            {
                throw new InvalidOperationException("TestSocketFactory can only create 1 server, see comment");
                // Clients use Server endpoint to connect, so a 2nd server can't be started from the same TestSocketFactory
                // if multiple server are needed then it would require multiple instances of TestSocketFactory
            }
            return new TestSocket(serverEndpoint);
        }

        public override EndPoint GetBindEndPoint()
        {
            return default;
        }

        public override EndPoint GetConnectEndPoint(string address = null, ushort? port = null)
        {
            return serverEndpoint;
        }
    }
}
