using Mirage;

public struct MyStructData {}

[NetworkMessage]
public struct MyMessage
{
    public MyStructData structField;
}
