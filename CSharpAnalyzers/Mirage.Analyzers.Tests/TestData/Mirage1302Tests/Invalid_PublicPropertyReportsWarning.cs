using Mirage;

[NetworkMessage]
public struct MyMessage
{
    public int id;
    public string {|#0:MyProperty|} { get; set; }
}
