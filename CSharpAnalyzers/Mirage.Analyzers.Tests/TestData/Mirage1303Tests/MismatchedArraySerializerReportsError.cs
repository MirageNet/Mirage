using Mirage.Serialization;

public struct CustomType {}

public static class CustomSerialization
{
    public static void {|#0:WriteCustomArray|}(this NetworkWriter writer, CustomType[] value) {}
}
