using Mirage;
using UnityEngine;

[NetworkMessage]
public struct MyMessage
{
    public GameObject goField;
    public Transform transformField;
    public NetworkIdentity identityField;
}
