using Mirage.Serialization;

public struct CustomType
{
    public int value;
}

public static class CustomSerialization
{
    public static CustomType {|#0:ReadCustomType|}(this NetworkReader reader) => default;
}
