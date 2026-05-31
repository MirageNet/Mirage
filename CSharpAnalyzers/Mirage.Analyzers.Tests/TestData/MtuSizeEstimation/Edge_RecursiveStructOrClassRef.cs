using Mirage;

[NetworkMessage]
public class RecursiveMessage
{
    public RecursiveMessage Parent;
    public int Value;
}
