using Mirage;
using Mirage.Serialization;

public struct CustomLengthType
{
    public string Name;
}

public static class CustomLengthSerialization
{
    public static void WriteCustomLengthType(this NetworkWriter writer, CustomLengthType value, int maxLength)
    {
        writer.WriteString(value.Name);
    }

    public static CustomLengthType ReadCustomLengthType(this NetworkReader reader, int maxLength)
    {
        return new CustomLengthType { Name = reader.ReadString() };
    }
}
