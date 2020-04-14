using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Mirror.Tests
{
    public class TestAuthenticator : NetworkAuthenticator
    {
        public void OnServerAuthenticateInternalExpose(INetworkConnection conn)
        {
            OnServerAuthenticateInternal(conn);
        }

        public void OnClientAuthenticateInternalExpose(INetworkConnection conn)
        {
            OnClientAuthenticateInternal(conn);
        }
    }

    [TestFixture]
    public class NetworkAuthenticatorTest : ClientServerSetup<MockComponent>
    {
        NetworkClient client;
        NetworkServer server;

        GameObject gameObject;
        TestAuthenticator testAuthenticator;
        INetworkConnection conn;
        int count;

        [SetUp]
        public void SetupTest()
        {
            gameObject = new GameObject();

            client = gameObject.AddComponent<NetworkClient>();
            server = gameObject.AddComponent<NetworkServer>();
            testAuthenticator = gameObject.AddComponent<TestAuthenticator>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(gameObject);
            count = 0;
        }

        
        void InvokedMethod(INetworkConnection conn)
        {
            count++;
        }

        [Test]
        public void OnServerAuthenticateTest()
        {
            testAuthenticator.OnServerAuthenticated += InvokedMethod;

            testAuthenticator.OnServerAuthenticate(conn);

            Assert.That(count, Is.EqualTo(1));
        }

        [Test]
        public void OnServerAuthenticateInternalTest()
        {
            testAuthenticator.OnServerAuthenticated += InvokedMethod;

            testAuthenticator.OnServerAuthenticateInternalExpose(conn);

            Assert.That(count, Is.EqualTo(1));
        }

        [Test]
        public void OnClientAuthenticateTest()
        {
            testAuthenticator.OnClientAuthenticated += InvokedMethod;

            testAuthenticator.OnClientAuthenticate(conn);

            Assert.That(count, Is.EqualTo(1));
        }

        [Test]
        public void OnClientAuthenticateInternalTest()
        {
            testAuthenticator.OnClientAuthenticated += InvokedMethod;

            testAuthenticator.OnClientAuthenticateInternalExpose(conn);

            Assert.That(count, Is.EqualTo(1));
        }

        [Test]
        public void ClientOnValidateTest()
        {
            Assert.That(client.authenticator, Is.EqualTo(testAuthenticator));
        }

        [Test]
        public void ServerOnValidateTest()
        {
            Assert.That(server.authenticator, Is.EqualTo(testAuthenticator));
        }
    }
}
