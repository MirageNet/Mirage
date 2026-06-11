using Mirage;

namespace Mirage.Snippets.Callbacks
{
    public class NetworkBehaviourCallbacks : NetworkBehaviour
    {
        // CodeEmbed-Start: network-behaviour-callbacks
        void Awake()
        {
            Identity.OnStartServer.AddListener(MyStartServer);
            Identity.OnStartClient.AddListener(MyStartClient);
            Identity.OnStartLocalPlayer.AddListener(MyStartLocalPlayer);
        }

        void MyStartServer()
        {
            // ...
        }

        void MyStartClient()
        {
            // ...
        }

        void MyStartLocalPlayer()
        {
            // ...
        }
        // CodeEmbed-End: network-behaviour-callbacks
    }
}
