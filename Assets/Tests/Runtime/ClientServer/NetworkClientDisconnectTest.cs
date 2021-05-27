using System.Collections;
using Mirage.SocketLayer;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.ClientServer
{
    public class NetworkClientDisconnectTest : ClientServerSetup<MockComponent>
    {
        Config config = new Config();

        public override void ExtraSetup()
        {
            // todo set config timeout here
        }

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
            connectionToClient.Disconnect();

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
    }

    public class NetworkClientConnectFailedTest : ClientServerSetup<MockComponent>
    {
        Config config = new Config();

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
        public IEnumerator DisconnectEventWhenFull()
        {
            // todo set server max connections to 0, then try to connect..
            Assert.Ignore("not implemented");
            int called = 0;
            client.Disconnected.AddListener((reason) =>
            {
                called++;
                Assert.That(reason, Is.EqualTo(ClientStoppedReason.ServerFull));
            });

            yield return null;

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
}
