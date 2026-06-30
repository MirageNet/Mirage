using Mirage;

public class MyClassData {}

[NetworkMessage]
public struct MyMessage
{
    public MyClassData {|#0:classField|};
}
