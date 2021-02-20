using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Text;
using System.Net;
using System;
using Object = UnityEngine.Object;
using System.Linq;
using Cysharp.Threading.Tasks;
using Mirage.KCP;
using NSubstitute;
using System.Collections.Generic;

namespace Mirage.Tests
{
    [TestFixture(typeof(KcpTransport), new[] { "kcp" }, "kcp://localhost", 7777)]
    public class TransportTests<T> where T : Transport
    {
        #region SetUp

        private T transport;
        private GameObject transportObj;
        private readonly Uri uri;
        private readonly int port;
        private readonly string[] scheme;

        public TransportTests(string[] scheme, string uri, int port)
        {
            this.scheme = scheme;
            this.uri = new Uri(uri);
            this.port = port;
        }

        IConnection clientConnection;
        IConnection serverConnection;


        Queue<(byte[] data, int channel)> clientMessages;
        Queue<(byte[] data, int channel)> serverMessages;

        UniTask listenTask;

        [UnitySetUp]
        public IEnumerator Setup() => UniTask.ToCoroutine(async () =>
        {
            transportObj = new GameObject();

            transport = transportObj.AddComponent<T>();

            transport.Connected.AddListener((connection) =>
                serverConnection = connection);

            listenTask = transport.ListenAsync();
            clientConnection = await transport.ConnectAsync(uri);

            await UniTask.WaitUntil(() => serverConnection != null);

            clientMessages = new Queue<(byte[], int)>();
            serverMessages = new Queue<(byte[], int)>();

            clientConnection.MessageReceived += (data, channel) =>
            {
                clientMessages.Enqueue((data.ToArray(), channel));
            };
            serverConnection.MessageReceived += (data, channel) =>
            {
                serverMessages.Enqueue((data.ToArray(), channel));
            };

        });


        [UnityTearDown]
        public IEnumerator TearDown() => UniTask.ToCoroutine(async () =>
        {
            clientConnection.Disconnect();
            serverConnection.Disconnect();
            transport.Disconnect();

            await listenTask;
            Object.Destroy(transportObj);
        });

        #endregion

        [Test]
        public void ClientToServerTest()
        {
            Encoding utf8 = Encoding.UTF8;
            string message = "Hello from the client";
            byte[] data = utf8.GetBytes(message);

            clientConnection.Send(new ArraySegment<byte>(data));

            transport.Poll();

            Assert.That(serverMessages.Dequeue().data, Is.EquivalentTo(data));
        }

        [Test]
        public void EndpointAddress()
        {
            // should give either IPv4 or IPv6 local address
            var endPoint = (IPEndPoint)serverConnection.GetEndPointAddress();

            IPAddress ipAddress = endPoint.Address;

            if (ipAddress.IsIPv4MappedToIPv6)
            {
                // mono IsLoopback seems buggy,
                // it does not detect loopback with mapped ipv4->ipv6 addresses
                // so map it back down to IPv4
                ipAddress = ipAddress.MapToIPv4();
            }

            Assert.That(IPAddress.IsLoopback(ipAddress), "Expected loopback address but got {0}", ipAddress);
            // random port
        }

        [Test]
        public void ClientToServerMultipleTest()
        {
            Encoding utf8 = Encoding.UTF8;
            string message = "Hello from the client 1";
            byte[] data = utf8.GetBytes(message);
            clientConnection.Send(new ArraySegment<byte>(data));

            string message2 = "Hello from the client 2";
            byte[] data2 = utf8.GetBytes(message2);
            clientConnection.Send(new ArraySegment<byte>(data2));

            transport.Poll();

            Assert.That(serverMessages.Dequeue().data, Is.EquivalentTo(data));
            Assert.That(serverMessages.Dequeue().data, Is.EquivalentTo(data2));
        }

        [Test]
        public void ServerToClientTest()
        {
            Encoding utf8 = Encoding.UTF8;
            string message = "Hello from the server";
            byte[] data = utf8.GetBytes(message);
            serverConnection.Send(new ArraySegment<byte>(data));

            transport.Poll();
            Assert.That(clientMessages.Dequeue().data, Is.EquivalentTo(data));
        }

        [Test]
        public void DisconnectServerTest()
        {
            Action disconnectMock = Substitute.For<Action>();
            clientConnection.Disconnected += disconnectMock;

            serverConnection.Disconnect();
            transport.Poll();

            disconnectMock.Received().Invoke();
        }

        [Test]
        public void DisconnectClientTest()
        {
            Action disconnectMock = Substitute.For<Action>();
            serverConnection.Disconnected += disconnectMock;

            clientConnection.Disconnect();
            transport.Poll();

            disconnectMock.Received().Invoke();
        }

        [Test]
        public void DisconnectClientTest2()
        {
            Action disconnectMock = Substitute.For<Action>();
            clientConnection.Disconnected += disconnectMock;

            clientConnection.Disconnect();
            transport.Poll();

            disconnectMock.Received().Invoke();
        }

        [Test]
        public void TestServerUri()
        {
            Uri serverUri = transport.ServerUri().First();

            Assert.That(serverUri.Port, Is.EqualTo(port));
            Assert.That(serverUri.Host, Is.EqualTo(Dns.GetHostName()).IgnoreCase);
            Assert.That(serverUri.Scheme, Is.EqualTo(uri.Scheme));
        }

        [Test]
        public void TestScheme()
        {
            Assert.That(transport.Scheme, Is.EquivalentTo(scheme));
        }
    }
}

