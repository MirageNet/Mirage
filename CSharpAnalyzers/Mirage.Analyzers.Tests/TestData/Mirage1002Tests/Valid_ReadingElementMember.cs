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
        int val = mySyncList[0].Value;
    }
}
