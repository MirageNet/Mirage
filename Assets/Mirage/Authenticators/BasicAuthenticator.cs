using Cysharp.Threading.Tasks;
using Mirage.Authentication;

namespace Mirage.Authenticators
{
    public class BasicAuthenticator : NetworkAuthenticatorBase<BasicAuthenticator.JoinMessage>
    {
        public string serverCode;

        // called on server to validate
        public override UniTask<AuthenticationResult> Authenticate(JoinMessage message)
        {
            AuthenticationResult result = default;
            result.Success = serverCode == message.serverCode;
            return UniTask.FromResult(result);
        }

        // called on client to create message to send to server
        public override UniTask<JoinMessage> CreateAuthentication()
        {
            var msg = new JoinMessage
            {
                serverCode = serverCode,
            };

            return UniTask.FromResult(msg);
        }

        [NetworkMessage]
        public struct JoinMessage
        {
            public string serverCode;
        }
    }
}
