using Mirage;

[NetworkMessage]
public struct MyMessage
{
    public int id;
    private static string secretCode;
}
