using Mirage.Serialization;

public struct CustomType {}

public static class OuterClass
{
    public static class InnerClass
    {
        public static void {|#0:WriteCustomType|}(this NetworkWriter writer, CustomType value) {}
    }
}
