using Mirage;
using Mirage.Collections;

public class NestedClass
{
    public int Value;
}

public class MyClass
{
    public NestedClass Nested = new NestedClass();
}

public class MyBehaviour : NetworkBehaviour
{
    public readonly SyncList<MyClass> mySyncList = new SyncList<MyClass>();

    public void Modify()
    {
        {|#0:mySyncList[0]|}.Nested.Value = 10;
    }
}
