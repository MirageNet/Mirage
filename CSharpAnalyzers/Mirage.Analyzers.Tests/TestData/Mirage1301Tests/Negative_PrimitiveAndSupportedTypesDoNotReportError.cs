using Mirage;
using UnityEngine;
using System;

[NetworkMessage]
public struct ValidMessage
{
    public int myInt;
    public string myString;
    public Vector3 myVector;
    public Guid myGuid;
    public DateTime myDateTime;
    public byte[] myByteArray;
}
