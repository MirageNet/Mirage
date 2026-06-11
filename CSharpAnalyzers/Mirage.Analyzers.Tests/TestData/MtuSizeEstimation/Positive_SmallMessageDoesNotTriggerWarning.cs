using Mirage;
using UnityEngine;

[NetworkMessage]
public struct {|#0:SmallMessage|}
{
    public int id;
    public Vector3 position;
    public string name;
}
