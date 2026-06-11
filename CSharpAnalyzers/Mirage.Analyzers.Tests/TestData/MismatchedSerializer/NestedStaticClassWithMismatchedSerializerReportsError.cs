using Mirage.Serialization;

public struct CustomType {}

namespace OuterNamespace
{
    public static class InnerClass
    {
        public static void {|#0:WriteCustomType|}(this NetworkWriter writer, CustomType value) {}
    }
}
