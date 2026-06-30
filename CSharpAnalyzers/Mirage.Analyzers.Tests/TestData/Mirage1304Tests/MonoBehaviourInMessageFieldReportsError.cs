using Mirage;
using UnityEngine;

public class PlainMonoBehaviour : MonoBehaviour {}

[NetworkMessage]
public struct MessageWithMonoBehaviour
{
    public PlainMonoBehaviour {|#0:myMonoBehaviour|};
}
