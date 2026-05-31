using Mirage;
using UnityEngine;

[NetworkMessage]
public struct SmallMessage
{
    public int id;
    public Vector3 position;
    public string name;
}
