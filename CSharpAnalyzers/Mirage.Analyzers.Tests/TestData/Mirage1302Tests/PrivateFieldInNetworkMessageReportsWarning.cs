using Mirage;

[NetworkMessage]
public struct MyMessage
{
    public int id;
    private string {|#0:secretCode|};
}
