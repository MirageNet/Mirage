using System;

namespace Mirage.Authenticators.SessionId
{
    public interface ISessionIdStore
    {
        bool TryGetSession(out ClientSession session);
        void StoreSession(ClientSession session);
    }

    public class ClientSession
    {
        public DateTime Timeout;
        public byte[] Key;
    }

    internal class DefaultSessionIdStore : ISessionIdStore
    {
        private ClientSession _session;

        public void StoreSession(ClientSession session)
        {
            _session = session;
        }

        public bool TryGetSession(out ClientSession session)
        {
            session = _session;
            return _session != null;
        }
    }
}
