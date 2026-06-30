using Mirage.Serialization;

public struct CustomType {}

public static class CustomSerialization
{
    // Not extension methods (missing 'this')
    public static void WriteCustomType(NetworkWriter writer, CustomType value) {}
    public static CustomType ReadCustomType(NetworkReader reader) => default;
}
