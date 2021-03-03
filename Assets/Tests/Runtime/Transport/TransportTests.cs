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
using Random = UnityEngine.Random;

namespace Mirage.Tests
{
    [TestFixture(typeof(KcpTransport), new[] { "kcp" }, "kcp://localhost", 7777)]
    public class TransportTests<T> where T : Transport
    {
        #region SetUp

        private T serverTransport;
        private GameObject serverTransportObj;

        private T clientTransport;
        private GameObject clientTransportObj;
        private readonly Uri uri;
        private readonly int port;
        private readonly string[] scheme;

        byte[] data1;
        byte[] data2;

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
            serverTransportObj = new GameObject("Server Transport");
            serverTransport = serverTransportObj.AddComponent<T>();
            serverTransport.Connected.AddListener((connection) =>
                serverConnection = connection);
            listenTask = serverTransport.ListenAsync();

            clientTransportObj = new GameObject("Client Transport");
            clientTransport = clientTransportObj.AddComponent<T>();

            UniTask<IConnection> connectTask = clientTransport.ConnectAsync(uri).Timeout(TimeSpan.FromSeconds(2));

            while (!connectTask.Status.IsCompleted() || serverConnection == null)
            {
                serverTransport.Poll();
                clientTransport.Poll();
                await UniTask.Delay(10);
            }
            clientConnection = await connectTask;

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

            data1 = CreateRandomData();
            data2 = CreateRandomData();
        });

        private byte[] CreateRandomData()
        {
            byte[] data = new byte[Random.Range(10, 255)];
            for (int i = 0; i < data.Length; i++)
                data[i] = (byte)Random.Range(1, 255);
            return data;
        }

        [UnityTearDown]
        public IEnumerator TearDown() => UniTask.ToCoroutine(async () =>
        {
            clientConnection.Disconnect();
            serverConnection.Disconnect();
            serverTransport.Disconnect();

            await listenTask;
            Object.Destroy(serverTransportObj);
            Object.Destroy(clientTransportObj);
        });

        public async UniTask WaitForMessage()
        {
            while (clientMessages.Count == 0 && serverMessages.Count == 0)
            {
                serverTransport.Poll();
                clientTransport.Poll();
                await UniTask.Delay(10);
            }
        }

        #endregion

        [UnityTest]
        public IEnumerator ClientToServerTest() => UniTask.ToCoroutine(async () =>
        {
            clientConnection.Send(new ArraySegment<byte>(data1));
            await WaitForMessage();
            Assert.That(serverMessages.Dequeue().data, Is.EquivalentTo(data1));
        });

        [UnityTest]
        public IEnumerator ServerToClientTest() => UniTask.ToCoroutine(async () =>
        {
            serverConnection.Send(new ArraySegment<byte>(data1));
            await WaitForMessage();
            Assert.That(clientMessages.Dequeue().data, Is.EquivalentTo(data1));
        });

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

        [UnityTest]
        public IEnumerator ClientToServerMultipleTest() => UniTask.ToCoroutine(async () =>
        {
            clientConnection.Send(new ArraySegment<byte>(data1));
            clientConnection.Send(new ArraySegment<byte>(data2));

            await WaitForMessage();
            Assert.That(serverMessages.Dequeue().data, Is.EquivalentTo(data1));

            await WaitForMessage();
            Assert.That(serverMessages.Dequeue().data, Is.EquivalentTo(data2));
        });


        [Test]
        public void DisconnectServerTest()
        {
            Action disconnectMock = Substitute.For<Action>();
            clientConnection.Disconnected += disconnectMock;

            serverConnection.Disconnect();
            serverTransport.Poll();

            disconnectMock.Received().Invoke();
        }

        [Test]
        public void DisconnectClientTest()
        {
            Action disconnectMock = Substitute.For<Action>();
            serverConnection.Disconnected += disconnectMock;

            clientConnection.Disconnect();
            serverTransport.Poll();

            disconnectMock.Received().Invoke();
        }

        [Test]
        public void DisconnectClientTest2()
        {
            Action disconnectMock = Substitute.For<Action>();
            clientConnection.Disconnected += disconnectMock;

            clientConnection.Disconnect();
            serverTransport.Poll();

            disconnectMock.Received().Invoke();
        }

        [Test]
        public void TestServerUri()
        {
            Uri serverUri = serverTransport.ServerUri().First();

            Assert.That(serverUri.Port, Is.EqualTo(port));
            Assert.That(serverUri.Host, Is.EqualTo(Dns.GetHostName()).IgnoreCase);
            Assert.That(serverUri.Scheme, Is.EqualTo(uri.Scheme));
        }

        [Test]
        public void TestScheme()
        {
            Assert.That(serverTransport.Scheme, Is.EquivalentTo(scheme));
        }
    }
}

