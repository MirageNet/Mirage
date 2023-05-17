using System;
using System.Collections.Generic;
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
}
