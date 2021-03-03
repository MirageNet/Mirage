using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using Cysharp.Threading.Tasks;
using System;

using UnityEngine;
using Random = UnityEngine.Random;
using System.Linq;
using Mirage.KCP;
using System.Collections.Generic;
using NSubstitute;

namespace Mirage.Tests
{
    public class KcpTransportTest
    {
        public ushort port = 7896;

        KcpTransport serverTransport;
        KcpConnection clientConnection;

        KcpTransport clientTransport;
        KcpConnection serverConnection;

        Uri testUri;

        UniTask listenTask;

        byte[] data;

        Queue<(byte[] data, int channel)> clientMessages;
        Queue<(byte[] data, int channel)> serverMessages;

        [UnitySetUp]
        public IEnumerator Setup() => UniTask.ToCoroutine(async () =>
        {
            // each test goes in a different port
            // that way the transports can take some time to cleanup
            // without interfering with each other.
            port++;

            var serverGo = new GameObject("serverTransport", typeof(KcpTransport));
            serverTransport = serverGo.GetComponent<KcpTransport>();
            serverTransport.Port = port;
            // speed this up
            serverTransport.HashCashBits = 3;

            var clientGo = new GameObject("clientTransport", typeof(KcpTransport));
            clientTransport = clientGo.GetComponent<KcpTransport>();
            clientTransport.Port = port;
            // speed this up
            clientTransport.HashCashBits = 3;


            serverTransport.Connected.AddListener(connection => serverConnection = (KcpConnection)connection);
            listenTask = serverTransport.ListenAsync();

            var uriBuilder = new UriBuilder
            {
                Host = "localhost",
                Scheme = "kcp",
                Port = port
            };

            testUri = uriBuilder.Uri;

            UniTask<IConnection> connectTask = clientTransport.ConnectAsync(uriBuilder.Uri).Timeout(TimeSpan.FromSeconds(2));

            // If we don't poll the transports,  they won't open the connection as they don't process
            // data on their own
            while (!connectTask.Status.IsCompleted() || serverConnection == null)
            {
                serverTransport.Poll();
                clientTransport.Poll();

                await UniTask.Delay(1);
            }

            clientConnection = (KcpConnection)await connectTask;

            // for our tests,  lower the timeout to just 0.1s
            // so that the tests run quickly.
            serverConnection.Timeout = 500;
            clientConnection.Timeout = 500;

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

            data = new byte[Random.Range(10, 255)];
            for (int i=0; i< data.Length; i++)
                data[i] = (byte)Random.Range(1, 255);
        });

        [UnityTearDown]
        public IEnumerator TearDown() => UniTask.ToCoroutine(async () =>
        {
            clientConnection?.Disconnect();
            serverConnection?.Disconnect();
            serverTransport.Disconnect();
            clientTransport.Disconnect();

            await listenTask;
            UnityEngine.Object.Destroy(serverTransport.gameObject);
            UnityEngine.Object.Destroy(clientTransport.gameObject);
            // wait a frame so object will be destroyed
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

        // A Test behaves as an ordinary method
        [Test]
        public void Connect()
        {
            Assert.That(clientConnection, Is.Not.Null);
            Assert.That(serverConnection, Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator ReceivedBytes() => UniTask.ToCoroutine( async () => {
            long received = serverTransport.ReceivedBytes;
            Assert.That(received, Is.GreaterThan(0), "Must have received some bytes to establish the connection");

            clientConnection.Send(new ArraySegment<byte>(data));

            await WaitForMessage();
            Assert.That(serverTransport.ReceivedBytes, Is.GreaterThan(received + data.Length), "Client sent data,  we should have received");

        });

        [UnityTest]
        public IEnumerator SentBytes() => UniTask.ToCoroutine( async () => {
            long sent = serverTransport.SentBytes;
            Assert.That(sent, Is.GreaterThan(0), "Must have received some bytes to establish the connection");

            serverConnection.Send(new ArraySegment<byte>(data));
            await WaitForMessage();
            Assert.That(serverTransport.SentBytes, Is.GreaterThan(sent + data.Length), "Client sent data,  we should have received");

        });

        [UnityTest]
        public IEnumerator SendUnreliableDataFromServer() => UniTask.ToCoroutine( async () => {
            serverConnection.Send(new ArraySegment<byte>(data), Channel.Unreliable);
            await WaitForMessage();
            Assert.That(clientMessages.Dequeue().channel, Is.EqualTo(Channel.Unreliable));
        });

        [UnityTest]
        public IEnumerator SendUnreliableDataFromClient() => UniTask.ToCoroutine( async () => {
            clientConnection.Send(new ArraySegment<byte>(data), Channel.Unreliable);
            await WaitForMessage();
            Assert.That(serverMessages.Dequeue().channel, Is.EqualTo(Channel.Unreliable));
        });


        [UnityTest]
        public IEnumerator DisconnectServerFromIdle() => UniTask.ToCoroutine(async () =>
        {
            Action disconnectMock = Substitute.For<Action>();

            serverConnection.Disconnected += disconnectMock;

            await UniTask.Delay(1000);
            serverTransport.Poll();
            disconnectMock.Received().Invoke();

        });

        [UnityTest]
        public IEnumerator DisconnectClientFromIdle() => UniTask.ToCoroutine(async () =>
        {
            Action disconnectMock = Substitute.For<Action>();

            clientConnection.Disconnected += disconnectMock;

            await UniTask.Delay(1000);
            serverTransport.Poll();
            disconnectMock.Received().Invoke();
        });

        [Test]
        public void TestServerUri()
        {
            Uri serverUri = serverTransport.ServerUri().First();

            Assert.That(serverUri.Port, Is.EqualTo(port));
            Assert.That(serverUri.Scheme, Is.EqualTo(testUri.Scheme));
        }

        [Test]
        public void IsSupportedTest()
        {
            Assert.That(serverTransport.Supported, Is.True);
        }

        [UnityTest]
        public IEnumerator ConnectionsDontLeak() => UniTask.ToCoroutine(async () =>
       {
           serverConnection.Disconnect();

           while (serverTransport.connections.Count > 0)
           {
               serverTransport.Poll();
               await UniTask.Delay(10);
           }

           Assert.That(serverTransport.connections, Is.Empty);
       });
    }
}
