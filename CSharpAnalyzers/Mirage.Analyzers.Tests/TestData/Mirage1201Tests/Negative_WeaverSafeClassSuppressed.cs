using Mirage;
using Mirage.Serialization;

[WeaverSafeClass]
public class SafeClassData {}

[NetworkMessage]
public struct MyMessage
{
    public SafeClassData safeClassField;
}

public static class SafeClassDataSerializer
{
    public static void Write(this NetworkWriter writer, SafeClassData value) {}
    public static SafeClassData Read(this NetworkReader reader) => default;
}
