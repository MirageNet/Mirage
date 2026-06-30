using Mirage;
using Mirage.Collections;

public class MyClass
{
    public int Value;
}

public class MyBehaviour : NetworkBehaviour
{
    public readonly SyncDictionary<int, MyClass> mySyncDict = new SyncDictionary<int, MyClass>();

    public void Modify()
    {
        {|#0:mySyncDict[1]|}.Value = 10;
    }
}
