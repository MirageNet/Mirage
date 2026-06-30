using Mirage;

[NetworkMessage]
public struct MyMessage
{
    public int id;
    internal string {|#0:internalCode|};
    protected float {|#1:protectedCode|};
}
