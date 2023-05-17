namespace Mirage.Authentication
{
    /// <summary>
    /// Result from Authentication, Use static methods to create new instance
    /// </summary>
    public struct AuthenticationResult
    {
        public bool Success { get; private set; }

        /// <summary>
        /// Which Authenticator gave success 
        /// </summary>
        public INetworkAuthenticator Authenticator { get; private set; }

        /// <summary>
        /// Auth data from Success, will be set on INetworkPlayer
        /// </summary>
        public object Data { get; private set; }
        /// <summary>
        /// Can be reason for Success of fail
        /// </summary>
        public string Reason { get; private set; }


        public static AuthenticationResult CreateSuccess(string reason)
        {
            return new AuthenticationResult
            {
                Success = true,
                Reason = reason,
            };
        }
        public static AuthenticationResult CreateSuccess(INetworkAuthenticator authenticator, object data)
        {
            return new AuthenticationResult
            {
                Success = true,
                Reason = "",
                Authenticator = authenticator,
                Data = data
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

