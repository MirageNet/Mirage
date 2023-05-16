using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Mirage.Authentication;
using UnityEngine;

namespace Mirage.Authenticators.SessionId
{
    public class SessionIdAuthenticator : NetworkAuthenticatorBase<SessionKeyMessage>
    {
        [Tooltip("how many bytes to use for session ID")]
        public int SessionIDLength = 32;
        [Tooltip("How long ID is valid for, in minutes. 1440 => 1 day")]
        public int TimeoutMinutes = 1440;

        /// <summary>
        /// Set on client to save key somewhere. For example as a cookie on webgl
        /// <para>
        /// By default it is just stored in memory
        /// </para>
        /// </summary>
        public ISessionIdStore ClientIdStore = new DefaultSessionIdStore();

        /// <summary>
        /// Active sessions on server
        /// </summary>
        private readonly Dictionary<SessionKey, SessionData> _sessions = new Dictionary<SessionKey, SessionData>();
        /// <summary>
        /// Key for player
        /// </summary>
        private readonly Dictionary<INetworkPlayer, SessionKey> _playerKeys = new Dictionary<INetworkPlayer, SessionKey>();

        protected override AuthenticationResult Authenticate(SessionKeyMessage message)
        {
            var key = new SessionKey(message.SessionKey);
            if (_sessions.TryGetValue(key, out var sessionData))
            {
                // check timeout
                if (DateTime.Now > sessionData.Timeout)
                {
                    _sessions.Remove(key);
                    return AuthenticationResult.CreateFail("Session has timed out");
                }


                return AuthenticationResult.CreateSuccess("Valid Session", this, sessionData);

            }
            else
            {
                return AuthenticationResult.CreateFail("No session ID found");
            }
        }

        protected override SessionKeyMessage CreateAuthentication()
        {
            if (ClientIdStore.TryGetSession(out var session))
            {
                return new SessionKeyMessage
                {
                    SessionKey = new ArraySegment<byte>(session.Key),
                };
            }
            else
            {
                throw new InvalidOperationException("No Session, make sure to check session exists before calling SendAuthentication");
            }
        }

        internal struct SessionKey : IEquatable<SessionKey>
        {
            private ArraySegment<byte> _id;
            private int _hash;

            public SessionKey(byte[] array) : this(new ArraySegment<byte>(array)) { }
            public SessionKey(ArraySegment<byte> bytes)
            {
                _id = bytes;
                _hash = CalculateHash(bytes);
            }

            public bool Equals(SessionKey other)
            {
                var count = _id.Count;
                if (count != other._id.Count)
                    return false;

                var array1 = _id.Array;
                var offset1 = _id.Offset;
                var array2 = other._id.Array;
                var offset2 = other._id.Offset;

                for (var i = 0; i < count; i++)
                {
                    if (array1[offset1 + i] != array2[offset2 + i])
                        return false;
                }
                return true;
            }

            public override int GetHashCode()
            {
                return _hash;
            }

            private static int CalculateHash(ArraySegment<byte> bytes)
            {
                var offset = bytes.Offset;
                var count = bytes.Count;
                var array = bytes.Array;

                unchecked
                {
                    var hash = StringHash.EmptyString;
                    for (var i = offset; i < count; i++)
                    {
                        hash = (hash * 31) + array[i];
                    }
                    return hash;
                }
            }
        }
    }

    /// <summary>
    /// Creates a session to be used by <see cref="SessionIdAuthenticator"/>
    /// </summary>
    public class CreateSession : MonoBehaviour
    {
        public NetworkServer Server;
        public NetworkClient Client;
        public SessionIdAuthenticator Authenticator;

        [Tooltip("Is the player required to be authenticated by another Authenticator before starting Session")]
        public bool RequestAuthenticated = true;

        public void Start()
        {
            Client.Authenticated.AddListener(ClientAuthenticated);
            Client.Connected.AddListener(ClientConnected);
        }

        private void ClientConnected(INetworkPlayer player)
        {
            if (Authenticator.ClientIdStore.TryGetSession(out var session))
            {
                // if before timeout, then use it to authenticate
                if (DateTime.Now < session.Timeout)
                    SendAuthentication(session);
            }
        }

        private void SendAuthentication(ClientSession session)
        {
            var msg = new SessionKeyMessage
            {
                SessionKey = new ArraySegment<byte>(session.Key)
            };
            Authenticator.SendAuthentication(Client, msg);
        }

        private void ClientAuthenticated(INetworkPlayer player)
        {
            if (existingSession == null) { }


        }

        public struct RequestSession
        {
            public bool RefreshExisting;
        }



        private void ClientSaveToken(INetworkPlayer player, IdMessage message)
        {
            var array = message.SessionId.ToArray();
            Debug.Assert(array.Length == SessionIDLength, "Server send unexpected session id length. This could mean that server was re-build but client was not");
            ClientIdStore.SetSessionId(array);
        }
        private void Reconnect(INetworkPlayer player, IdMessage message)
        {
            if (!_sessions.TryGetValue(player, out var sessionData))
            {
                player.Send<SessionRejected>();
                return;
            }

            if (!CompareSessionId(sessionData.SessionId, message.SessionId))
            {
                player.Send<SessionRejected>();
                return;
            }

            player.Send<SessionAccepted>();
            ServerAccept(player);
        }

        private bool CompareSessionId(byte[] sessionId, ArraySegment<byte> segment)
        {
            // Check if the lengths are equal
            if (sessionId.Length != segment.Count)
            {
                return false;
            }

            // Compare the bytes
            var array = segment.Array;
            var offset = segment.Offset;
            for (var i = 0; i < sessionId.Length; i++)
            {
                if (sessionId[i] != array[offset + i])
                {
                    return false;
                }
            }

            return true;
        }

        private void AssignNewSession(INetworkPlayer player, RequestNewIDMessage message)
        {
            var sessionId = GenerateSessionId();

            var sessionData = new SessionData
            {
                SessionId = sessionId,
                Timeout = DateTime.Now.AddMinutes(TimeoutMinutes)
            };

            _sessions[player] = sessionData;

            player.Send(new IdMessage { SessionId = new ArraySegment<byte>(sessionId) });
            ServerAccept(player);
        }

        private byte[] GenerateSessionId()
        {
            // Generate a random value
            var value = new byte[SessionIDLength];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(value);
            }

            return value;
        }

        public override void ServerAuthenticate(INetworkPlayer player)
        {
            // nothing, wait for message
        }
        public override void ClientAuthenticate(INetworkPlayer player)
        {
            var sessionId = ClientIdStore.GetSessionId();
            if (sessionId == null)
                player.Send<RequestNewIDMessage>();
            else
                player.Send(new IdMessage { SessionId = new ArraySegment<byte>(sessionId) });
        }
    }
}
