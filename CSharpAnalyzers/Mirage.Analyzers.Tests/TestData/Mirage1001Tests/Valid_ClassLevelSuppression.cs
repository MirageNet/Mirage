using Mirage;

[WeaverSafeClass]
public class MyClass {}

public class MyBehaviour : NetworkBehaviour
{
    [SyncVar]
    public MyClass MySyncVar;
}
