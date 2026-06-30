using Mirage;
using UnityEngine;

public class MyNetworkBehaviour : NetworkBehaviour {}

public class PlayerCombat : NetworkBehaviour
{
    [ServerRpc, RateLimit]
    public void CmdInteract(MyNetworkBehaviour target)
    {
    }
}
