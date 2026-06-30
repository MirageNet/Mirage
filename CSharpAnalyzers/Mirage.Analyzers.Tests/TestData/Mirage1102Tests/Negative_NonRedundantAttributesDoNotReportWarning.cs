using Mirage;
using UnityEngine;

public class PlayerCombat : NetworkBehaviour
{
    [ServerRpc]
    public void CmdFireWeapon(int weaponId)
    {
    }

    [ClientRpc]
    public void RpcPlayExplosion(Vector3 position)
    {
    }

    [Server]
    public void LocalServerOnlyLogic()
    {
    }

    [Client]
    public void LocalClientOnlyLogic()
    {
    }
}
