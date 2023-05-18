using System.Collections;
using Mirage.Authentication;
using Mirage.Authenticators.SessionId;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.Authentication
{
    public class SessionIdAuthenticatorTests : ClientServerSetup
    {
        protected AuthenticatorSettings serverSettings;
        protected SessionIdAuthenticator serverAuthenticator;
        protected MockAuthenticator serverMockAuthenticator;
        protected CreateSession serverCreateSession;

        protected AuthenticatorSettings clientSettings;
        protected SessionIdAuthenticator clientAuthenticator;
        protected MockAuthenticator clientMockAuthenticator;
        protected CreateSession clientCreateSession;

        private static void Setup(GameObject go, ref AuthenticatorSettings settings, ref SessionIdAuthenticator sessionId, ref MockAuthenticator mock, ref CreateSession createSession)
        {
            settings = go.AddComponent<AuthenticatorSettings>();
            sessionId = go.AddComponent<SessionIdAuthenticator>();
            mock = go.AddComponent<MockAuthenticator>();
            createSession = go.AddComponent<CreateSession>();

            createSession.Authenticator = sessionId;

            settings.Authenticators.Add(mock);
            settings.Authenticators.Add(sessionId);
        }

        protected override void ExtraServerSetup()
        {
            base.ExtraServerSetup();

            Setup(serverGo, ref serverSettings, ref serverAuthenticator, ref serverMockAuthenticator, ref serverCreateSession);

            server.Authenticator = serverSettings;
            serverCreateSession.Server = server;
        }

        protected override void ExtraClientSetup(IClientInstance instance)
        {
            base.ExtraClientSetup(instance);
            var client = instance.Client;

            Setup(clientGo, ref clientSettings, ref clientAuthenticator, ref clientMockAuthenticator, ref clientCreateSession);

            client.Authenticator = clientSettings;
            clientCreateSession.Client = client;
        }


        [UnityTest]
        public IEnumerator ClientConnected_UseExistingValidSession_Success()
        {
            Assert.Fail();
            yield return null;
        }

        [UnityTest]
        public IEnumerator ClientConnected_UseExistingExpiredSession_Fail()
        {
            Assert.Fail();
            yield return null;
        }

        [UnityTest]
        public IEnumerator ClientConnected_NoExistingSession_RequestSession()
        {
            Assert.Fail();
            yield return null;
        }

        [UnityTest]
        public IEnumerator ClientAuthenticated_NoExistingSession_ReceiveSessionKey()
        {
            Assert.Fail();
            yield return null;
        }

        [UnityTest]
        public IEnumerator HandleRequestSession_CreateOrRefreshSession_SendSessionKey()
        {
            Assert.Fail();
            yield return null;
        }

        [UnityTest]
        public IEnumerator CheckRefresh_SessionNeedsRefreshing_RequestSession()
        {
            Assert.Fail();
            yield return null;
        }

        [UnityTest]
        public IEnumerator CheckRefresh_SessionDoesNotNeedRefreshing_NoRequest()
        {
            Assert.Fail();
            yield return null;
        }

        [UnityTest]
        public IEnumerator CheckRefresh_ClientInactive_NoRequest()
        {
            Assert.Fail();
            yield return null;
        }

        [UnityTest]
        public IEnumerator RequestSession_ReceiveSessionKey_StoreSession()
        {
            Assert.Fail();
            yield return null;
        }
    }
}
