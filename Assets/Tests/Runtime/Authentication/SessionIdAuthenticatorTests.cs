using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Mirage.Authentication;
using Mirage.Authenticators.SessionId;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.Authentication
{
    public class SessionIdAuthenticatorTests : ClientServerSetup
    {
        protected AuthenticatorSettings _serverSettings;
        protected SessionIdAuthenticator _serverAuthenticator;
        protected MockAuthenticator _serverMockAuthenticator;
        protected CreateSession _serverCreateSession;

        protected AuthenticatorSettings _clientSettings;
        protected SessionIdAuthenticator _clientAuthenticator;
        protected MockAuthenticator _clientMockAuthenticator;

        protected List<INetworkPlayer> _serverAuthCalls;
        protected List<INetworkPlayer> _clientAuthCalls;

        protected override bool SpawnCharacterOnConnect => false;

        private static void Setup(GameObject go, ref AuthenticatorSettings settings, ref SessionIdAuthenticator sessionId, ref MockAuthenticator mock)
        {
            settings = go.AddComponent<AuthenticatorSettings>();
            sessionId = go.AddComponent<SessionIdAuthenticator>();
            mock = go.AddComponent<MockAuthenticator>();

            settings.Authenticators.Add(mock);
            settings.Authenticators.Add(sessionId);
        }

        protected override void ExtraServerSetup()
        {
            base.ExtraServerSetup();

            Setup(serverGo, ref _serverSettings, ref _serverAuthenticator, ref _serverMockAuthenticator);

            server.Authenticator = _serverSettings;

            // we always want to add CreateSession to server to listen for messages
            _serverCreateSession = serverGo.AddComponent<CreateSession>();
            _serverCreateSession.Authenticator = _serverAuthenticator;

            _serverAuthCalls = new List<INetworkPlayer>();
            server.Authenticated.AddListener(p => _serverAuthCalls.Add(p));
        }

        protected override void ExtraClientSetup(IClientInstance instance)
        {
            base.ExtraClientSetup(instance);
            var client = instance.Client;

            Setup(instance.GameObject, ref _clientSettings, ref _clientAuthenticator, ref _clientMockAuthenticator);

            client.Authenticator = _clientSettings;
            _clientAuthCalls = new List<INetworkPlayer>();
            client.Authenticated.AddListener(p => _clientAuthCalls.Add(p));
        }


        [Test]
        public void DoesNotSendSessionKeyWhenNotSet()
        {
            Assert.That(_serverAuthCalls, Is.Empty);
            Assert.That(_clientAuthCalls, Is.Empty);

            Assert.That(serverPlayer.IsAuthenticated, Is.False);
            Assert.That(clientPlayer.IsAuthenticated, Is.False);
        }

        [Test]
        public void CreateOrRefreshSessionReturnsBytes()
        {
            var key = _serverAuthenticator.CreateOrRefreshSession(serverPlayer);

            Assert.That(key.Count, Is.EqualTo(_serverAuthenticator.SessionIDLength));
            Assert.That(key.Count(x => x != 0), Is.GreaterThan(1), "atleast 1 non-zero. Just to make sure it didn't return empty array");
        }

        [UnityTest]
        public IEnumerator ReturnSuccessWhenGivenKey() => UniTask.ToCoroutine(async () =>
        {
            var key = _serverAuthenticator.CreateOrRefreshSession(serverPlayer);
            var result = await _serverAuthenticator.AuthenticateAsync(new SessionKeyMessage { SessionKey = key });

            Assert.That(result.Success, Is.True);
            Assert.That(result.Authenticator, Is.TypeOf<SessionIdAuthenticator>());
            Assert.That(result.Data, Is.TypeOf<SessionData>());
        });

        [UnityTest]
        public IEnumerator SessionDataContainsExistingData() => UniTask.ToCoroutine(async () =>
        {
            // set existing data
            var netPlayer = (NetworkPlayer)serverPlayer;
            var mockAuth = new PlayerAuthentication(_serverMockAuthenticator, new MockAuthenticator.MockData());
            netPlayer.SetAuthentication(mockAuth, true);

            // request key for player
            // this should save auth data in session data
            var key = _serverAuthenticator.CreateOrRefreshSession(serverPlayer);
            var result = await _serverAuthenticator.AuthenticateAsync(new SessionKeyMessage { SessionKey = key });

            Assert.That(result.Data, Is.TypeOf<SessionData>());
            var sessionData = (SessionData)result.Data;

            Assert.That(sessionData.PlayerAuthentication, Is.EqualTo(mockAuth));
        });

        [Test]
        public void CanGetMockDataUsingGetData()
        {
            var mockAuth = new PlayerAuthentication(_serverMockAuthenticator, new MockAuthenticator.MockData());
            var sessionData = new SessionData()
            {
                PlayerAuthentication = mockAuth
            };
            var sessionAuth = new PlayerAuthentication(_serverAuthenticator, sessionData);

            // can get both data out using GetData Method
            Assert.That(sessionAuth.GetData<SessionData>(), Is.EqualTo(sessionData));
            Assert.That(sessionAuth.GetData<MockAuthenticator.MockData>(), Is.EqualTo(mockAuth.Data));
        }

        [UnityTest]
        public IEnumerator ReturnFailWhenKeyIsNull() => UniTask.ToCoroutine(async () =>
        {
            var result = await _serverAuthenticator.AuthenticateAsync(new SessionKeyMessage { SessionKey = default });

            Assert.That(result.Success, Is.False);
            Assert.That(result.Reason, Is.EqualTo("No key"));
        });

        [UnityTest]
        public IEnumerator ReturnFailWhenKeyIsNotFound() => UniTask.ToCoroutine(async () =>
        {
            var key = new ArraySegment<byte>(new byte[_serverAuthenticator.SessionIDLength]);
            var result = await _serverAuthenticator.AuthenticateAsync(new SessionKeyMessage { SessionKey = key });

            Assert.That(result.Success, Is.False);
            Assert.That(result.Reason, Is.EqualTo("No session ID found"));
        });

        [UnityTest]
        public IEnumerator SendsKeyAutomaticallyIfSet()
        {
            // get key and store key
            var key = _serverAuthenticator.CreateOrRefreshSession(serverPlayer);
            _clientAuthenticator.ClientIdStore.StoreSession(new ClientSession { Key = key.ToArray(), Timeout = DateTime.Now.AddMinutes(1000) });

            // add CreateSession
            var createSession = clientGo.AddComponent<CreateSession>();
            createSession.Authenticator = _clientAuthenticator;
            createSession.Client = client;


            Assert.That(_serverAuthCalls, Is.Empty);
            Assert.That(_clientAuthCalls, Is.Empty);

            Assert.That(serverPlayer.IsAuthenticated, Is.False);
            Assert.That(clientPlayer.IsAuthenticated, Is.False);

            // wait for start and messages
            yield return null;
            yield return null;

            Assert.That(_serverAuthCalls, Has.Count.EqualTo(1));
            Assert.That(_serverAuthCalls[0], Is.EqualTo(serverPlayer));

            Assert.That(serverPlayer.IsAuthenticated, Is.True);
            Assert.That(serverPlayer.Authentication, Is.Not.Null);
            Assert.That(serverPlayer.Authentication.Authenticator, Is.TypeOf<SessionIdAuthenticator>());
            Assert.That(serverPlayer.Authentication.Data, Is.TypeOf<SessionData>());

            // client needs extra frame to receive message from server
            yield return null;

            Assert.That(_clientAuthCalls, Has.Count.EqualTo(1));
            Assert.That(_clientAuthCalls[0], Is.EqualTo(client.Player));

            Assert.That(client.Player.IsAuthenticated, Is.True);
            Assert.That(client.Player.Authentication, Is.Not.Null);
            Assert.That(client.Player.Authentication.Authenticator, Is.TypeOf<SessionIdAuthenticator>());
        }

        [UnityTest]
        public IEnumerator ClientConnected_UseExistingValidSession_Success()
        {
            // add create session
            var createSession = clientGo.AddComponent<CreateSession>();
            createSession.Authenticator = _clientAuthenticator;
            createSession.Client = client;

            Assert.That(_clientAuthenticator.ClientIdStore.TryGetSession(out var _), Is.False);

            // auth using mock
            _clientMockAuthenticator.SendAuthentication(client, new MockAuthenticator.MockMessage());

            Assert.That(_serverAuthCalls, Has.Count.EqualTo(0));
            Assert.That(_clientAuthCalls, Has.Count.EqualTo(0));

            yield return null;
            yield return null;

            Assert.That(_serverAuthCalls, Has.Count.EqualTo(1));
            Assert.That(_clientAuthCalls, Has.Count.EqualTo(1));

            Assert.That(client.Player.Authentication.Authenticator, Is.TypeOf<MockAuthenticator>());
            var serverPlayer = server.Players.First();
            Assert.That(serverPlayer.Authentication.Authenticator, Is.TypeOf<MockAuthenticator>());
            var firstData = serverPlayer.Authentication.GetData<MockAuthenticator.MockData>();
            Assert.That(serverPlayer.Authentication.Data, Is.TypeOf<MockAuthenticator.MockData>());

            // wait 2 more frames for CreateSession to request key
            yield return null;
            yield return null;
            Assert.That(_clientAuthenticator.ClientIdStore.TryGetSession(out var _), Is.True);

            // disconnect, and reconnect
            client.Disconnect();
            yield return null;
            yield return null;

            client.Connect();
            yield return null;
            yield return null;

            Assert.That(_serverAuthCalls, Has.Count.EqualTo(2));
            Assert.That(_clientAuthCalls, Has.Count.EqualTo(2));

            // get player again, reference would have changed
            serverPlayer = server.Players.First();

            Assert.That(client.Player.Authentication.Authenticator, Is.TypeOf<SessionIdAuthenticator>());
            Assert.That(serverPlayer.Authentication.Authenticator, Is.TypeOf<SessionIdAuthenticator>());
            Assert.That(serverPlayer.Authentication.Data, Is.TypeOf<SessionData>());
            Assert.That(serverPlayer.Authentication.GetData<MockAuthenticator.MockData>(), Is.EqualTo(firstData), "Should get same data again");
        }


        [UnityTest]
        public IEnumerator ClientRequestNewKeyAfterTimeout()
        {
            // first connect normally, we get sessionId
            // add create session
            var createSession = clientGo.AddComponent<CreateSession>();
            createSession.Authenticator = _clientAuthenticator;
            createSession.Client = client;

            Assert.That(_clientAuthenticator.ClientIdStore.TryGetSession(out var _), Is.False);

            // auth using mock
            _clientMockAuthenticator.SendAuthentication(client, new MockAuthenticator.MockMessage());

            yield return null;
            yield return null;
            // wait 2 more frames for CreateSession to request key
            yield return null;
            yield return null;
            Assert.That(_clientAuthenticator.ClientIdStore.TryGetSession(out var session1), Is.True);

            // change timeout
            session1.Timeout = DateTime.Now;

            yield return null;
            yield return null;

            Assert.That(_clientAuthenticator.ClientIdStore.TryGetSession(out var session2), Is.True);
            Assert.That(session2, Is.Not.EqualTo(session1));
            Assert.That(session2.Timeout > session1.Timeout);
        }
    }
}
