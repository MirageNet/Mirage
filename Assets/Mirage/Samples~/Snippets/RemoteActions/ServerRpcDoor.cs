using UnityEngine;
using Mirage;
using INetworkPlayer = Mirage.Snippets.RemoteActions.ServerRpcDoor.IDummyNetworkPlayer;

namespace Mirage.Snippets.RemoteActions.ServerRpcDoor
{
    public interface IDummyNetworkPlayer : INetworkPlayer
    {
        NetworkIdentity identity { get; }
    }

    public class Player : MonoBehaviour
    {
        public bool hasDoorKey;
    }

    // CodeEmbed-Start: server-rpc-door
    public enum DoorState : byte
    {
        Open, Closed
    }

    public class Door : NetworkBehaviour
    {
        [SyncVar]
        public DoorState doorState;

        [ServerRpc(requireAuthority = false)]
        public void CmdSetDoorState(DoorState newDoorState, INetworkPlayer sender = null)
        {
            if (sender.Identity.GetComponent<Player>().hasDoorKey)
                doorState = newDoorState;
        }
    }
    // CodeEmbed-End: server-rpc-door
}
