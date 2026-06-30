using Mirage;
using Mirage.Serialization;

[NetworkMessage]
public struct ValidMessage
{
    public CustomType customValue;
}

public struct CustomType
{
    // type with invalid field but custom writers
    public int[,] myArray;
}

public static class CustomSerialization
{
    public static void WriteCustomType(this NetworkWriter writer, CustomType value) { }
    public static CustomType ReadCustomType(this NetworkReader reader) => default;
}