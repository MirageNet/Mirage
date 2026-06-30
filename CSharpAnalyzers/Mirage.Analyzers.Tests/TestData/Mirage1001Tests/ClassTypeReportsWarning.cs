using Mirage;

public class MyClass {}

public class MyBehaviour : NetworkBehaviour
{
    [SyncVar]
    public MyClass {|#0:MySyncVar|} { get; set; }
}
