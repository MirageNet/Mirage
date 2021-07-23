using System;
using System.Collections.Generic;
using System.Linq;
using Mirage.SocketLayer;
using NSubstitute;
using UnityEngine;

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
        public static Dictionary<IEndPoint, TestSocket> allSockets = new Dictionary<IEndPoint, TestSocket>();

        /// <summary>
        /// Can be useful to fake timeouts or dropped messages
        /// </summary>
        public static bool StopAllMessages;

        public static bool EndpointInUse(IEndPoint endPoint) => allSockets.ContainsKey(endPoint);

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


        public readonly IEndPoint endPoint;

        readonly Queue<Packet> received = new Queue<Packet>();
        public List<Packet> Sent = new List<Packet>();

        public readonly string name;

        public TestSocket(string name, IEndPoint endPoint = null)
        {
            this.name = name;
            this.endPoint = endPoint ?? TestEndPoint.CreateSubstitute();
        }


        void ISocket.Bind(IEndPoint endPoint)
        {
            AddThisSocket();
        }

        void ISocket.Connect(IEndPoint endPoint)
        {
            AddThisSocket();
        }

        void ISocket.Close()
        {
            allSockets.Remove(endPoint);
        }

        bool ISocket.Poll()
        {
            return received.Count > 0;
        }

        int ISocket.Receive(byte[] buffer, out IEndPoint endPoint)
        {
            Packet next = received.Dequeue();
            endPoint = next.endPoint;
            int length = next.length;

            Buffer.BlockCopy(next.data, 0, buffer, 0, length);
            return length;
        }

        void ISocket.Send(IEndPoint remoteEndPoint, byte[] packet, int length)
        {
            AddThisSocket();

            if (!allSockets.TryGetValue(remoteEndPoint, out TestSocket other))
            {
                // other socket might have been closed
                return;
            }

            // create copy because data is from buffer
            byte[] clone = packet.Take(length).ToArray();
            Sent.Add(new Packet
            {
                endPoint = remoteEndPoint,
                data = clone,
                length = length
            });

            // mark as sent, but not as received
            if (StopAllMessages)
                return;

            other.received.Enqueue(new Packet
            {
                endPoint = endPoint,
                data = clone,
                length = length
            });
        }

        public struct Packet
        {
            public IEndPoint endPoint;
            public byte[] data;
            public int length;
        }
    }

    public static class TestEndPoint
    {
        public static IEndPoint CreateSubstitute()
        {
            IEndPoint endpoint = Substitute.For<IEndPoint>();
            endpoint.CreateCopy().Returns(endpoint);
            return endpoint;
        }
    }

    public class TestSocketFactory : MonoBehaviour, ISocketFactory
    {
        public IEndPoint serverEndpoint = TestEndPoint.CreateSubstitute();

        int clientNameIndex;
        int serverNameIndex;
        public ISocket CreateClientSocket()
        {
            return new TestSocket($"Client {clientNameIndex++}");
        }

        public ISocket CreateServerSocket()
        {
            if (TestSocket.EndpointInUse(serverEndpoint))
            {
                throw new InvalidOperationException("TestSocketFactory can only create 1 server, see comment");
                // Clients use Server endpoint to connect, so a 2nd server can't be started from the same TestSocketFactory
                // if multiple server are needed then it would require multiple instances of TestSocketFactory
            }
            return new TestSocket($"Server {serverNameIndex++}", serverEndpoint);
        }

        public IEndPoint GetBindEndPoint()
        {
            return default;
        }

        public IEndPoint GetConnectEndPoint(string address = null, ushort? port = null)
        {
            return serverEndpoint;
        }
    }
}
