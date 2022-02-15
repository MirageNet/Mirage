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
        readonly Config config = new Config()
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
            int called = 0;
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
            int called = 0;
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
            int called = 0;
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
            (ISocket clientSocket, IEndPoint serverEndPoint) = GetSocketAndEndPoint();
            byte[] badMessage = CreateInvalidPacket();

            clientSocket.Send(serverEndPoint, badMessage, 20);

            int called = 0;
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
            IEndPoint clientEndPoint = server.Players.First().Connection.EndPoint;
            TestSocket clientSocket = TestSocket.allSockets[clientEndPoint];

            IEndPoint serverEndPoint = ((TestSocketFactory)server.SocketFactory).serverEndpoint;

            return (clientSocket, serverEndPoint);
        }

        private static byte[] CreateInvalidPacket()
        {
            byte[] packet = new byte[20];
            int offset = 0;
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

            int called = 0;
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

            int called = 0;
            client.Disconnected.AddListener((reason) =>
            {
                called++;
                Assert.That(reason, Is.EqualTo(ClientStoppedReason.ConnectingTimeout));
            });

            // wait longer than timeout
            var config = new Config();
            float endTime = Time.time + (config.ConnectAttemptInterval * config.MaxConnectAttempts * 1.5f);
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

            int called = 0;
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

            int called = 0;
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
