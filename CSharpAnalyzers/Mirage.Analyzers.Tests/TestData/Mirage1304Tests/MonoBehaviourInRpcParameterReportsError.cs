using Mirage;
using UnityEngine;

public class PlainMonoBehaviour : MonoBehaviour {}

public class PlayerCombat : NetworkBehaviour
{
    [ServerRpc]
    public void CmdInteract(PlainMonoBehaviour {|#0:target|})
    {
    }
}
