using Mirage;

namespace Mirage.Snippets.Authentication
{
    public interface IBasicAuthenticator
    {
        // CodeEmbed-Start: basic-authenticator-sendcode
        void SendCode(NetworkClient client, string serverCode = null);
        // CodeEmbed-End: basic-authenticator-sendcode
    }
}
