using System.Collections;
using System.Linq;
using Mirage.SocketLayer;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.ClientServer.DisconnectTests
{
    public class NetworkClientDisconnectTest : ClientServerSetup<MockComponent>
    {
        private readonly Config config = new Config()
        {
            // lower timeout so tests doesn't wait too long
            TimeoutDuration = 2,
        };
        protected override Config ClientConfig => config;

        public override void ExtraTearDown()
        {
            TestSocket.StopAllMessages = false;
        }

        [UnityTest]
        public IEnumerator DisconnectEventWhenClientDisconnects()
        {
            var called = 0;
            client.Disconnected.AddListener((reason) =>
            {
                called++;
                Assert.That(reason, Is.EqualTo(ClientStoppedReason.LocalConnectionClosed));
                Debug.Log("Disconnected");
            });
            client.Disconnect();

            // wait 2 frames so that messages can go from client->server->client
            yield return null;
            yield return null;

            Assert.That(called, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator DisconnectEventWhenServerDisconnects()
        {
            var called = 0;
            client.Disconnected.AddListener((reason) =>
            {
                called++;
                Assert.That(reason, Is.EqualTo(ClientStoppedReason.RemoteConnectionClosed));
            });

            // server's object
            serverPlayer.Disconnect();

            // wait 2 frames so that messages can go from client->server->client
            yield return null;
            yield return null;

            Assert.That(called, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator DisconnectEventWhenTimeout()
        {
            var called = 0;
            client.Disconnected.AddListener((reason) =>
            {
                called++;
                Assert.That(reason, Is.EqualTo(ClientStoppedReason.Timeout));
            });

            TestSocket.StopAllMessages = true;
            // wait longer than timeout
            yield return new WaitForSeconds(config.TimeoutDuration * 1.5f);

            Assert.That(called, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator DisconnectEventWhenSentInvalidPacket()
        {
            (var clientSocket, var serverEndPoint) = GetSocketAndEndPoint();
            var badMessage = CreateInvalidPacket();

            clientSocket.Send(serverEndPoint, badMessage, 20);

            var called = 0;
            client.Disconnected.AddListener((reason) =>
            {
                called++;
                Assert.That(reason, Is.EqualTo(ClientStoppedReason.InvalidPacket));
            });

            // wait 2 frames so that messages can go from client->server->client
            yield return null;
            yield return null;

            Assert.That(called, Is.EqualTo(1));
        }

        private (ISocket clientSocket, IEndPoint serverEndPoint) GetSocketAndEndPoint()
        {
            var clientEndPoint = server.Players.First().Connection.EndPoint;
            var clientSocket = TestSocket.allSockets[clientEndPoint];

            var serverEndPoint = ((TestSocketFactory)server.SocketFactory).serverEndpoint;

            return (clientSocket, serverEndPoint);
        }

        private static byte[] CreateInvalidPacket()
        {
            var packet = new byte[20];
            var offset = 0;
            ByteUtils.WriteByte(packet, ref offset, (byte)PacketType.ReliableFragment);
            // reliable order header
            offset += 2;
            offset += 2;
            offset += 8;
            offset += 2;

            // byte fragment index over limit in config
            ByteUtils.WriteByte(packet, ref offset, 200);

            return packet;
        }
    }

    public class NetworkClientConnectFailedFullServerTest : ClientServerSetup<MockComponent>
    {
        protected override Config ServerConfig => new Config { MaxConnections = 0 };
        protected override bool AutoConnectClient => false;

        [UnityTest]
        public IEnumerator DisconnectEventWhenFull()
        {
            client.Connect("localhost");

            var called = 0;
            client.Disconnected.AddListener((reason) =>
            {
                called++;
                Assert.That(reason, Is.EqualTo(ClientStoppedReason.ServerFull));
            });

            // wait 2 frames so that messages can go from client->server->client
            yield return null;
            yield return null;

            Assert.That(called, Is.EqualTo(1));
        }
    }

    public class NetworkClientConnectFailedTest : ClientServerSetup<MockComponent>
    {
        protected override bool AutoConnectClient => false;

        public override void ExtraTearDown()
        {
            TestSocket.StopAllMessages = false;
        }

        [UnityTest]
        public IEnumerator DisconnectEventWhenTimeout()
        {
            TestSocket.StopAllMessages = true;
            client.Connect("localhost");

            var called = 0;
            client.Disconnected.AddListener((reason) =>
            {
                called++;
                Assert.That(reason, Is.EqualTo(ClientStoppedReason.ConnectingTimeout));
            });

            // wait longer than timeout
            var config = new Config();
            var endTime = Time.time + (config.ConnectAttemptInterval * config.MaxConnectAttempts * 1.5f);
            while (Time.time < endTime)
            {
                if (client.IsConnected)
                {
                    // early exit if failed
                    Assert.Fail("Client should not have connected");
                }
                yield return null;
            }

            Assert.That(called, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator DisconnectEventWhenCanceled()
        {
            client.Connect("localhost");

            var called = 0;
            client.Disconnected.AddListener((reason) =>
            {
                called++;
                Assert.That(reason, Is.EqualTo(ClientStoppedReason.ConnectingCancel));
            });

            // stop connecting
            client.Disconnect();

            // wait 2 frames so that messages can go from client->server->client
            yield return null;
            yield return null;

            Assert.That(called, Is.EqualTo(1));
        }
    }

    public class NetworkClientConnectFailedBadKeyTest : ClientServerSetup<MockComponent>
    {
        protected override Config ServerConfig => new Config { key = "Server Key" };
        protected override Config ClientConfig => new Config { key = "Client Key" };
        protected override bool AutoConnectClient => false;

        [UnityTest]
        public IEnumerator DisconnectEventWhenFull()
        {
            client.Connect("localhost");

            var called = 0;
            client.Disconnected.AddListener((reason) =>
            {
                called++;
                Assert.That(reason, Is.EqualTo(ClientStoppedReason.KeyInvalid));
            });

            // wait 2 frames so that messages can go from client->server->client
            yield return null;
            yield return null;

            Assert.That(called, Is.EqualTo(1));
        }
    }
}
