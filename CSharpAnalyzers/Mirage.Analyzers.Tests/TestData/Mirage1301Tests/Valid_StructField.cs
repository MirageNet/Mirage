using Mirage;
using Mirage.Serialization;

public struct CustomType
{
    public int value;
}

[NetworkMessage]
public struct ValidMessage
{
    public CustomType customValue;
}
