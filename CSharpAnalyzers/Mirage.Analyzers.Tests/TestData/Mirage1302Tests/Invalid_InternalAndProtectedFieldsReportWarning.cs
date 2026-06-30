using Mirage;

[NetworkMessage]
public struct MyMessage
{
    public int id;
    internal string {|#0:internalCode|};
}

[NetworkMessage]
public class MyClassMessage
{
    protected float {|#1:protectedCode|};
}
