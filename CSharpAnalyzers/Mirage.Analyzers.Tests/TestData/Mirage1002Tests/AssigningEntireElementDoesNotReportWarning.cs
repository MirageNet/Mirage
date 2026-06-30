using Mirage;
using Mirage.Collections;

[WeaverSafeClass]
public class MyStruct
{
    public int Value;
}

public class MyBehaviour : NetworkBehaviour
{
    public readonly SyncList<MyStruct> mySyncList = new SyncList<MyStruct>();

    public void Modify()
    {
        mySyncList[0] = new MyStruct { Value = 10 };
    }
}
