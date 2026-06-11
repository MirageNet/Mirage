using Mirage;

namespace Mirage.Snippets.RemoteActions.RpcExampleChangeName
{
    // CodeEmbed-Start: rpc-example-change-name
    public class Player : NetworkBehaviour
    {
        [SyncVar]
        public string PlayerName;

        [ServerRpc]
        public void RpcChangeName(string newName)
        {
            PlayerName = newName;
        }
    }
    // CodeEmbed-End: rpc-example-change-name
}
