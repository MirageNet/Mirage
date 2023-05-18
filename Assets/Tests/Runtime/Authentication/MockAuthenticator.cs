using Mirage.Authentication;

namespace Mirage.Tests.Runtime.Authentication
{
    public class MockAuthenticator : NetworkAuthenticator<MockAuthenticator.MockMessage>
    {
        protected override AuthenticationResult Authenticate(MockMessage message)
        {
            return AuthenticationResult.CreateSuccess(this, new MockData { });
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
