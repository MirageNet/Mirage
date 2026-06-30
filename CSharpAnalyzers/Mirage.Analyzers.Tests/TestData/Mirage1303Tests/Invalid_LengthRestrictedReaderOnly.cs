using Mirage;
using Mirage.Serialization;

public struct CustomLengthType
{
    public string Name;
}

public static class CustomLengthSerialization
{
    public static CustomLengthType {|#0:ReadCustomLengthType|}(this NetworkReader reader, int maxLength)
    {
        return new CustomLengthType { Name = reader.ReadString() };
    }
}
