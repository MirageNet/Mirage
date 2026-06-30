using Mirage;
using Mirage.Serialization;

public struct CustomType
{
    public int value;
}

public static class CustomSerialization
{
    public static void WriteCustomType(this NetworkWriter writer, CustomType value) {}
    public static CustomType ReadCustomType(this NetworkReader reader) => default;
}

[NetworkMessage]
public struct ValidMessage
{
    public CustomType customValue;
}
