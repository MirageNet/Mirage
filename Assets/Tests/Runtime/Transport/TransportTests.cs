using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Text;
using System.IO;
using System.Net;
using System;
using Object = UnityEngine.Object;
using System.Linq;
using Cysharp.Threading.Tasks;
using Mirror.KCP;
using NSubstitute;

using TaskChannel = Cysharp.Threading.Tasks.Channel;

namespace Mirror.Tests
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

        Channel<byte[]> serverMessages;
        Channel<byte[]> clientMessages;

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

            serverMessages = TaskChannel.CreateSingleConsumerUnbounded<byte[]>();
            clientMessages = TaskChannel.CreateSingleConsumerUnbounded<byte[]>();

            clientConnection.MessageReceived += (data, channel) =>
            {
                clientMessages.Writer.TryWrite(data.ToArray());
            };
            serverConnection.MessageReceived += (data, channel) =>
            {
                serverMessages.Writer.TryWrite(data.ToArray());
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

        [UnityTest]
        public IEnumerator ClientToServerTest() => UniTask.ToCoroutine(async () =>
        {
            Encoding utf8 = Encoding.UTF8;
            string message = "Hello from the client";
            byte[] data = utf8.GetBytes(message);
            clientConnection.Send(new ArraySegment<byte>(data));

            byte[] received = await Receive(serverConnection);
            Assert.That(received, Is.EqualTo(data));
        });

        private UniTask<byte[]> Receive(IConnection connection)
        {
            var completionSource = new UniTaskCompletionSource<byte[]>();

            void HandleMessage(ArraySegment<byte> data, int channel)
            {
                connection.MessageReceived -= HandleMessage;

                byte[] received = data.ToArray();

                completionSource.TrySetResult(received);
            }
            connection.MessageReceived += HandleMessage;

            return completionSource.Task;
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

        [UnityTest]
        public IEnumerator ClientToServerMultipleTest() => UniTask.ToCoroutine(async () =>
        {
            Encoding utf8 = Encoding.UTF8;
            string message = "Hello from the client 1";
            byte[] data = utf8.GetBytes(message);
            clientConnection.Send(new ArraySegment<byte>(data));

            string message2 = "Hello from the client 2";
            byte[] data2 = utf8.GetBytes(message2);
            clientConnection.Send(new ArraySegment<byte>(data2));

            byte[] received = await serverMessages.Reader.ReadAsync();
            byte[] received2 = await serverMessages.Reader.ReadAsync();
            Assert.That(received2, Is.EqualTo(data2));
        });

        [UnityTest]
        public IEnumerator ServerToClientTest() => UniTask.ToCoroutine(async () =>
        {
            Encoding utf8 = Encoding.UTF8;
            string message = "Hello from the server";
            byte[] data = utf8.GetBytes(message);
            serverConnection.Send(new ArraySegment<byte>(data));

            byte[] received = await clientMessages.Reader.ReadAsync();
            Assert.That(received, Is.EqualTo(data));
        });

        [UnityTest]
        public IEnumerator DisconnectServerTest() => UniTask.ToCoroutine(async () =>
        {
            var disconnectMock = Substitute.For<Action>();
            clientConnection.Disconnected += disconnectMock;
            serverConnection.Disconnect();
            disconnectMock.Received().Invoke();
        });

        [UnityTest]
        public IEnumerator DisconnectClientTest() => UniTask.ToCoroutine(async () =>
        {
            var disconnectMock = Substitute.For<Action>();
            serverConnection.Disconnected += disconnectMock;
            clientConnection.Disconnect();
            disconnectMock.Received().Invoke();
        });

        [UnityTest]
        public IEnumerator DisconnectClientTest2() => UniTask.ToCoroutine(async () =>
        {
            var disconnectMock = Substitute.For<Action>();
            clientConnection.Disconnected += disconnectMock;
            clientConnection.Disconnect();
            disconnectMock.Received().Invoke();
       });

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

