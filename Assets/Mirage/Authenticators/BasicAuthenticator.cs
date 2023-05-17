using Mirage.Authentication;

namespace Mirage.Authenticators
{
    public class BasicAuthenticator : NetworkAuthenticatorBase<BasicAuthenticator.JoinMessage>
    {
        public string serverCode;

        // called on server to validate
        protected override AuthenticationResult Authenticate(JoinMessage message)
        {
            if (serverCode == message.serverCode)
            {
                return AuthenticationResult.CreateSuccess(this, null);
            }
            else
            {
                return AuthenticationResult.CreateFail("Server code invalid", this);
            }
        }

        // called on client to create message to send to server
        public void SendCode(NetworkClient client, string serverCode = null)
        {
            var message = new JoinMessage
            {
                // use the argument or field if null
                serverCode = serverCode ?? this.serverCode
            };

            SendAuthentication(client, message);
        }

        [NetworkMessage]
        public struct JoinMessage
        {
            public string serverCode;
        }
    }
}
