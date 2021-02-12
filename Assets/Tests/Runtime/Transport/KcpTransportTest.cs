using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using Cysharp.Threading.Tasks;
using System;
using System.IO;
using NSubstitute;

using UnityEngine;
using Random = UnityEngine.Random;
using System.Linq;
using Mirror.KCP;
using TaskChannel = Cysharp.Threading.Tasks.Channel;

namespace Mirror.Tests
{
    public class KcpTransportTest
    {
        public ushort port = 7896;

        KcpTransport transport;
        KcpConnection clientConnection;
        KcpConnection serverConnection;

        Uri testUri;

        UniTask listenTask;

        byte[] data;

        Channel<(byte[], int)> serverMessages;
        Channel<(byte[], int)> clientMessages;

        [UnitySetUp]
        public IEnumerator Setup() => UniTask.ToCoroutine(async () =>
        {
            // each test goes in a different port
            // that way the transports can take some time to cleanup
            // without interfering with each other.
            port++;

            var transportGo = new GameObject("kcpTransport", typeof(KcpTransport));

            transport = transportGo.GetComponent<KcpTransport>();

            transport.Port = port;
            // speed this up
            transport.HashCashBits = 3;
       
            transport.Connected.AddListener(connection => serverConnection = (KcpConnection)connection);

            listenTask = transport.ListenAsync();

            var uriBuilder = new UriBuilder
            {
                Host = "localhost",
                Scheme = "kcp",
                Port = port
            };

            testUri = uriBuilder.Uri;

            clientConnection = (KcpConnection)await transport.ConnectAsync(uriBuilder.Uri);

            await UniTask.WaitUntil(() => serverConnection != null);

            serverMessages = TaskChannel.CreateSingleConsumerUnbounded<(byte[], int)>();
            clientMessages = TaskChannel.CreateSingleConsumerUnbounded<(byte[], int)>();

            clientConnection.MessageReceived += (data, channel) =>
            {
                clientMessages.Writer.TryWrite((data.ToArray(), channel));
            };
            serverConnection.MessageReceived += (data, channel) =>
            {
                serverMessages.Writer.TryWrite((data.ToArray(), channel));
            };


            // for our tests,  lower the timeout to just 0.1s
            // so that the tests run quickly.
            serverConnection.Timeout = 500;
            clientConnection.Timeout = 500;

            data = new byte[Random.Range(10, 255)];
            for (int i=0; i< data.Length; i++)
                data[i] = (byte)Random.Range(1, 255);
        });

        [UnityTearDown]
        public IEnumerator TearDown() => UniTask.ToCoroutine(async () =>
        {
            clientConnection?.Disconnect();
            serverConnection?.Disconnect();
            transport.Disconnect();

            await listenTask;
            UnityEngine.Object.Destroy(transport.gameObject);
            // wait a frame so object will be destroyed
        });

        // A Test behaves as an ordinary method
        [Test]
        public void Connect()
        {
            Assert.That(clientConnection, Is.Not.Null);
            Assert.That(serverConnection, Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator SendDataFromClient() => UniTask.ToCoroutine(async () =>
        {
            clientConnection.Send(new ArraySegment<byte>(data));
            (byte[] received, int channel) = await serverMessages.Reader.ReadAsync();
            Assert.That(received, Is.EquivalentTo(data));
        });

        [UnityTest]
        public IEnumerator SendDataFromServer() => UniTask.ToCoroutine(async () =>
        {
            serverConnection.Send(new ArraySegment<byte>(data));
            (byte[] received, int channel) = await clientMessages.Reader.ReadAsync();
            Assert.That(received, Is.EquivalentTo(data));
        });

        [UnityTest]
        public IEnumerator ReceivedBytes() => UniTask.ToCoroutine(async () =>
        {
            long received = transport.ReceivedBytes;
            Assert.That(received, Is.GreaterThan(0), "Must have received some bytes to establish the connection");
            clientConnection.Send(new ArraySegment<byte>(data));
            _ = await serverMessages.Reader.ReadAsync();
            Assert.That(transport.ReceivedBytes, Is.GreaterThan(received + data.Length), "Client sent data,  we should have received");
        });

        [UnityTest]
        public IEnumerator SentBytes() => UniTask.ToCoroutine(async () =>
        {
            long sent = transport.SentBytes;
            Assert.That(sent, Is.GreaterThan(0), "Must have received some bytes to establish the connection");
            serverConnection.Send(new ArraySegment<byte>(data));
            _ = await clientMessages.Reader.ReadAsync();
            Assert.That(transport.SentBytes, Is.GreaterThan(sent + data.Length), "Client sent data,  we should have received");
        });

        [UnityTest]
        public IEnumerator SendUnreliableDataFromServer() => UniTask.ToCoroutine(async () =>
        {
            serverConnection.Send(new ArraySegment<byte>(data), Channel.Unreliable);

            (byte[] received, int channel) = await clientMessages.Reader.ReadAsync();
            Assert.That(received, Is.EquivalentTo(data));
            Assert.That(channel, Is.EqualTo(Channel.Unreliable));
        });

        [UnityTest]
        public IEnumerator SendUnreliableDataFromClient() => UniTask.ToCoroutine(async () =>
        {
            clientConnection.Send(new ArraySegment<byte>(data), Channel.Unreliable);

            (byte[] received, int channel) = await serverMessages.Reader.ReadAsync();
            Assert.That(received, Is.EquivalentTo(data));
            Assert.That(channel, Is.EqualTo(Channel.Unreliable));
        });

        [UnityTest]
        public IEnumerator DisconnectServerFromIdle() => UniTask.ToCoroutine(async () =>
        {
            Action disconnected = Substitute.For<Action>();
            serverConnection.Disconnected += disconnected;

            await UniTask.WaitUntil(() => disconnected.ReceivedCalls().Any()).Timeout(TimeSpan.FromSeconds(2));
            disconnected.Received().Invoke();
        });

        [UnityTest]
        public IEnumerator DisconnectClientFromIdle() => UniTask.ToCoroutine(async () =>
        {
            Action disconnected = Substitute.For<Action>();
            clientConnection.Disconnected += disconnected;

            await UniTask.WaitUntil(() => disconnected.ReceivedCalls().Any()).Timeout(TimeSpan.FromSeconds(2));
            disconnected.Received().Invoke();
        });

        [Test]
        public void TestServerUri()
        {
            Uri serverUri = transport.ServerUri().First();

            Assert.That(serverUri.Port, Is.EqualTo(port));
            Assert.That(serverUri.Scheme, Is.EqualTo(testUri.Scheme));
        }

        [Test]
        public void IsSupportedTest()
        {
            Assert.That(transport.Supported, Is.True);
        }

        [UnityTest]
        public IEnumerator ConnectionsDontLeak() => UniTask.ToCoroutine(async () =>
        {
            Action disconnected = Substitute.For<Action>();
            serverConnection.Disconnected += disconnected;
            serverConnection.Disconnect();

            await UniTask.WaitUntil(() => disconnected.ReceivedCalls().Any()).Timeout(TimeSpan.FromSeconds(2));

            Assert.That(transport.connectedClients, Is.Empty);
        });
    }
}
