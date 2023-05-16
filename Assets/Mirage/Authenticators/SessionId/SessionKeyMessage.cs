using System;

namespace Mirage.Authenticators.SessionId
{
    public struct SessionKeyMessage
    {
        public ArraySegment<byte> SessionKey;
    }
}
