using Mirage;

public class MyClass {}

public class MyBehaviour : NetworkBehaviour
{
    [SyncVar]
    [WeaverSafeClass]
    public MyClass MySyncVar;
}
