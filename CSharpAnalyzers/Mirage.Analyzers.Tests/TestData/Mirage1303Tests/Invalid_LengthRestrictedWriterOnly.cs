using Mirage;
using Mirage.Serialization;

public struct CustomLengthType
{
    public string Name;
}

public static class CustomLengthSerialization
{
    public static void {|#0:WriteCustomLengthType|}(this NetworkWriter writer, CustomLengthType value, int maxLength)
    {
        writer.WriteString(value.Name);
    }
}
