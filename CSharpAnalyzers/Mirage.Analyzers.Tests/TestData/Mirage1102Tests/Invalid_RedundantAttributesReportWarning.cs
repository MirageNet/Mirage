using Mirage;
using UnityEngine;

public class PlayerCombat : NetworkBehaviour
{
    [{|#0:Server|}]
    [ServerRpc, RateLimit]
    public void CmdFireWeapon(int weaponId)
    {
    }

    [{|#1:Client|}]
    [ClientRpc]
    public void RpcPlayExplosion(Vector3 position)
    {
    }
}
