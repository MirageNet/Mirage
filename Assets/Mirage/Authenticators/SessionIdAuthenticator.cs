using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;

namespace Mirage.Authenticators.Session
{
    public interface ISessionIdStore
    {
        void SetSessionId(byte[] sessionId);
        byte[] GetSessionId();
    }
    public class SessionIdAuthenticator : NetworkAuthenticator
    {
        [Tooltip("how many bytes to use for session ID")]
        public int SessionIDLength = 32;
        [Tooltip("How long ID is valid for, in minutes. 1440 => 1 day")]
        public int TimeoutMinutes = 1440;

        /// <summary>
        /// Set on client to save key somewhere. For example as a cookie on webgl
        /// </summary>
        public ISessionIdStore ClientIdStore = new DefaultSessionIdStore();

        // Dictionary to store session IDs and associated data
        private readonly Dictionary<INetworkPlayer, SessionData> _sessions = new Dictionary<INetworkPlayer, SessionData>();

        public override void ServerSetup(NetworkServer server)
        {
            server.MessageHandler.RegisterHandler<RequestNewIDMessage>(AssignNewSession);
            server.MessageHandler.RegisterHandler<IdMessage>(Reconnect);
        }

        public override void ClientSetup(NetworkClient client)
        {
            client.MessageHandler.RegisterHandler<IdMessage>(ClientSaveToken);
            client.MessageHandler.RegisterHandler<SessionAccepted>((p, m) => ClientAccept(p));
            client.MessageHandler.RegisterHandler<SessionRejected>((p, m) => ClientReject(p));
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


        [NetworkMessage]
        private struct RequestNewIDMessage { }
        [NetworkMessage]
        private struct SessionAccepted { }
        [NetworkMessage]
        private struct SessionRejected { }
        [NetworkMessage]
        private struct IdMessage
        {
            public ArraySegment<byte> SessionId;
        }

        private class SessionData
        {
            public byte[] SessionId;
            public DateTime Timeout;
        }
        private class DefaultSessionIdStore : ISessionIdStore
        {
            private byte[] _clientSessionId;

            public byte[] GetSessionId()
            {
                return _clientSessionId;
            }

            public void SetSessionId(byte[] sessionId)
            {
                _clientSessionId = sessionId;
            }
        }
    }
}
