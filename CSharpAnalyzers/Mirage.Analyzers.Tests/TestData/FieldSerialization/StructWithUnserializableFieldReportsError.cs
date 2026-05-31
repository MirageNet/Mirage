using Mirage;
using System.Threading;

public struct NestedUnserializable
{
    public Thread threadField;
}

[NetworkMessage]
public struct MainMessage
{
    public NestedUnserializable {|#0:nestedField|};
}
