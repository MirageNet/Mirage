using Mirage;

[WeaverSafeClass]
public class SafeClassData {}

[NetworkMessage]
public struct MyMessage
{
    public SafeClassData safeClassField;
}
