using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Mirage.Authentication;
using Mirage.Logging;
using UnityEngine;

namespace Mirage.Authenticators.SessionId
{
    public class SessionIdAuthenticator : NetworkAuthenticator<SessionKeyMessage>
    {
        public const string NO_KEY_ERROR = "Empty key from client";
        public const string NOT_FOUND_ERROR = "No session found";
        private static readonly ILogger logger = LogFactory.GetLogger<SessionIdAuthenticator>();

        [Tooltip("how many bytes to use for session ID")]
        public int SessionIDLength = 32;
        [Tooltip("How long ID is valid for, in minutes. 1440 => 1 day")]
        public int TimeoutMinutes = 1440;
        private RNGCryptoServiceProvider _rng;

        private void Awake()
        {
            _rng = new RNGCryptoServiceProvider();
        }
        private void OnDestroy()
        {
            _rng.Dispose();
        }

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

        protected override AuthenticationResult Authenticate(INetworkPlayer player, SessionKeyMessage message)
        {
            if (message.SessionKey.Count == 0)
                return AuthenticationResult.CreateFail(NO_KEY_ERROR);

            var key = new SessionKey(message.SessionKey);
            if (!_sessions.TryGetValue(key, out var sessionData))
                return AuthenticationResult.CreateFail(NOT_FOUND_ERROR);

            // check timeout
            if (DateTime.Now > sessionData.Timeout)
            {
                _sessions.Remove(key);
                return AuthenticationResult.CreateFail("Session has timed out");
            }

            return AuthenticationResult.CreateSuccess("Valid Session", this, sessionData);
        }

        public ArraySegment<byte> CreateOrRefreshSession(INetworkPlayer player)
        {
            SessionData session;
            // get existing session
            if (_playerKeys.TryGetValue(player, out var oldKey))
            {
                if (logger.LogEnabled()) logger.Log($"Refreshing session for {player}");
                session = _sessions[oldKey];
                _sessions.Remove(oldKey);
            }
            // or create new
            else
            {
                if (logger.LogEnabled()) logger.Log($"Creating new session for {player}");
                session = new SessionData();
                session.PlayerAuthentication = player.Authentication;
            }

            // create new key
            var key = GenerateSessionKey();
            // set new timeout
            session.Timeout = DateTime.Now.AddMinutes(TimeoutMinutes);

            // set lookup with new key
            _sessions[key] = session;
            _playerKeys[player] = key;
            return key.Buffer;
        }

        private SessionKey GenerateSessionKey()
        {
            var key = new byte[SessionIDLength];
            _rng.GetBytes(key);
            return new SessionKey(key);
        }

        internal struct SessionKey : IEquatable<SessionKey>
        {
            public ArraySegment<byte> Buffer { get; }
            private readonly int _hash;

            public SessionKey(byte[] array) : this(new ArraySegment<byte>(array)) { }
            public SessionKey(ArraySegment<byte> bytes)
            {
                Buffer = bytes;
                _hash = CalculateHash(bytes);
            }

            public bool Equals(SessionKey other)
            {
                var count = Buffer.Count;
                if (count != other.Buffer.Count)
                    return false;

                var array1 = Buffer.Array;
                var offset1 = Buffer.Offset;
                var array2 = other.Buffer.Array;
                var offset2 = other.Buffer.Offset;

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
                    for (var i = 0; i < count; i++)
                    {
                        hash = (hash * 31) + array[i + offset];
                    }
                    return hash;
                }
            }
        }
    }
}
