using System;

namespace Mirage.Authenticators.SessionId
{
    [NetworkMessage]
    public struct SessionKeyMessage
    {
        public ArraySegment<byte> SessionKey;
    }

    [NetworkMessage]
    public struct RequestSessionMessage
    {
    }
}
