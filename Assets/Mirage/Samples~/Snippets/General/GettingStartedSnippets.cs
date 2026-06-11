using UnityEngine;

namespace Mirage.Snippets.General
{
    // CodeEmbed-Start: getting-started-client-authority
    public class GettingStartedClientAuthority : NetworkBehaviour
    {
        private void Update()
        {
            if (!IsLocalPlayer)
                return;

            // handle player input for movement
        }
    }
    // CodeEmbed-End: getting-started-client-authority

    // CodeEmbed-Start: getting-started-server-authority
    public class GettingStartedServerAuthority : NetworkBehaviour
    {
        private void Update()
        {
            if (!IsLocalPlayer)
                return;

            // handle player input for movement

            // You would call this command after handling input or you can send inputs directly to
            // server and let server buffer inputs up and do movements based on the buffered inputs.
            MovePlayer();
        }

        [ServerRpc]
        private void MovePlayer()
        {
            // We are now firing off some kind of movement all done by server.
        }
    }
    // CodeEmbed-End: getting-started-server-authority
}
