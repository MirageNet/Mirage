using Mirage;

[WeaverSafeClass]
public class RecursiveClass
{
    public RecursiveClass {|#0:self|};
}

[NetworkMessage]
public struct Message
{
    public RecursiveClass {|#1:recursiveField|};
}
