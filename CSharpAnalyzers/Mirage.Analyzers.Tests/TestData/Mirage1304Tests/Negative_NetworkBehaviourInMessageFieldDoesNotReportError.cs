using Mirage;
using UnityEngine;

public class MyNetworkBehaviour : NetworkBehaviour {}

[NetworkMessage]
public struct MessageWithNetworkBehaviour
{
    public MyNetworkBehaviour myNetworkBehaviour;
}
