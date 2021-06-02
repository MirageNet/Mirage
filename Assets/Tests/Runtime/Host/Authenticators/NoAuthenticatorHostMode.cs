namespace Mirage.Tests.Runtime.Host.Authenticators
{
    public class NoAuthenticatorHostMode : AuthenticatorHostModeBase
    {
        protected override void AddAuthenticator()
        {
            server.authenticator = null;
            client.authenticator = null;

        }
    }
}
