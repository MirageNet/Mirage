using Mirage;
using Mirage.Collections;

public class MyClass
{
    public int Value;
}

public class MyBehaviour : NetworkBehaviour
{
    public readonly SyncList<MyClass> mySyncList = new SyncList<MyClass>();

    public void Modify()
    {
        {|#0:mySyncList[0]|}.Value = 10;
    }
}
