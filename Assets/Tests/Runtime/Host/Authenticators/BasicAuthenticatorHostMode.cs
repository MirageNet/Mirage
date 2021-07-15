using Mirage.Authenticators;

namespace Mirage.Tests.Runtime.Host.Authenticators
{
    public class BasicAuthenticatorHostMode : AuthenticatorHostModeBase
    {
        protected override void AddAuthenticator()
        {
            BasicAuthenticator auth = networkManagerGo.AddComponent<BasicAuthenticator>();
            server.authenticator = auth;
            client.authenticator = auth;
            auth.serverCode = "1234";
        }
    }
}
