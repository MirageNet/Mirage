namespace Mirage.Authentication
{
    public struct AuthenticationResult
    {
        public bool Success;

        /// <summary>
        /// Which Authenticator gave success 
        /// </summary>
        public INetworkAuthenticator Authenticator;

        /// <summary>
        /// Auth data from Success, will be set on INetworkPlayer
        /// </summary>
        public object Data;
        /// <summary>
        /// Can be reason for Success of fail
        /// </summary>
        public string Reason;


        public static AuthenticationResult CreateSuccess(string reason)
        {
            return new AuthenticationResult
            {
                Success = true,
                Reason = reason,
            };
        }
        public static AuthenticationResult CreateSuccess(string reason, INetworkAuthenticator authenticator, object data)
        {
            return new AuthenticationResult
            {
                Success = true,
                Reason = reason,
                Authenticator = authenticator,
                Data = data
            };
        }
        public static AuthenticationResult CreateFail(string reason)
        {
            return new AuthenticationResult
            {
                Success = false,
                Reason = reason,
            };
        }
        public static AuthenticationResult CreateFail(string reason, INetworkAuthenticator authenticator)
        {
            return new AuthenticationResult
            {
                Success = false,
                Reason = reason,
                Authenticator = authenticator,
            };
        }
    }
}

