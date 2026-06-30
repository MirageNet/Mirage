using System;
using Mirage;

[NetworkMessage]
public struct MyMessage
{
    public int id;
    [NonSerialized]
    private string secretCode;
}
