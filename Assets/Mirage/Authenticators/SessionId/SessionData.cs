using System;
using Mirage.Authentication;

namespace Mirage.Authenticators.SessionId
{
    public class SessionData : IAuthenticationDataWrapper
    {
        public DateTime Timeout;
        public PlayerAuthentication PlayerAuthentication;

        object IAuthenticationDataWrapper.Inner => PlayerAuthentication.Data;
    }
}
