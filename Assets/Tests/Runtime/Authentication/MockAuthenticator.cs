using Mirage.Authentication;

namespace Mirage.Tests.Runtime.ClientServer
{
    public class MockAuthenticator : NetworkAuthenticator<MockAuthenticator.MockMesasge>
    {
        protected override AuthenticationResult Authenticate(MockMesasge message)
        {
            return AuthenticationResult.CreateSuccess(this, new MockData { });
        }

        public class MockData
        {

        }

        [NetworkMessage]
        public struct MockMesasge
        {

        }
    }
}
