using Mirage.Serialization;

public struct CustomType
{
    public int value;
}

public static class CustomSerialization
{
    public static void {|#0:WriteCustomType|}(this NetworkWriter writer, CustomType value) {}
}
