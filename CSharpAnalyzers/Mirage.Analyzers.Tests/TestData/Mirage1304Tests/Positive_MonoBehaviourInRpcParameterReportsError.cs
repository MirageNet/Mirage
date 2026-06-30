using Mirage;
using UnityEngine;

public class PlainMonoBehaviour : MonoBehaviour {}

public class PlayerCombat : NetworkBehaviour
{
    [ServerRpc, RateLimit]
    public void CmdInteract(PlainMonoBehaviour {|#0:target|})
    {
    }
}
