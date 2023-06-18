using Mirage.Authentication;

namespace Mirage.Tests.Runtime.Authentication
{
    public class MockAuthenticator : NetworkAuthenticator<MockAuthenticator.MockMessage>
    {
        public bool Success = true;
        public string FailReason = "Succcess is false";

        protected override AuthenticationResult Authenticate(INetworkPlayer player, MockMessage message)
        {
            if (Success)
                return AuthenticationResult.CreateSuccess(this, new MockData { });
            else
                return AuthenticationResult.CreateFail(FailReason, this);
        }

        public class MockData
        {

        }

        [NetworkMessage]
        public struct MockMessage
        {

        }
    }
}
