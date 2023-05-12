using System;

namespace Mirage.Authentication
{
    /// <summary>
    /// Wrapper message around auth message sent by a <see cref="NetworkAuthenticatorBase"/>
    /// <para>
    /// This type is used to that it can be receuved before player is authenticated.
    /// ALl AuthMessage will be handled by an Authenticator instead of the normal message handler
    /// </para>
    /// </summary>
    [NetworkMessage]
    public struct AuthMessage
    {
        public ArraySegment<byte> Payload;
    }
}

