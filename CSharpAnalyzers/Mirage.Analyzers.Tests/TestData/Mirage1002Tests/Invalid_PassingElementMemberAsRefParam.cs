using Mirage;
using Mirage.Collections;

[WeaverSafeClass]
public class MyClass
{
    public int Value;
}

public class MyBehaviour : NetworkBehaviour
{
    public readonly SyncList<MyClass> mySyncList = new SyncList<MyClass>();

    public void Modify()
    {
        Helper(ref {|#0:mySyncList[0]|}.Value);
    }

    private void Helper(ref int val)
    {
        val = 5;
    }
}
