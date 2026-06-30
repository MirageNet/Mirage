using Mirage;
using Mirage.Serialization;

[NetworkMessage]
[WeaverSafeClass]
public class {|#0:RecursiveMessage|}
{
    [WeaverSafeClass]
    public RecursiveMessage Parent;
    public int Value;
}

public static class RecursiveMessageSerializer
{
    public static void Write(this NetworkWriter writer, RecursiveMessage value) {}
    public static RecursiveMessage Read(this NetworkReader reader) => default;
}
