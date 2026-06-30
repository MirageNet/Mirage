using System.Collections.Generic;
using Mirage;

[WeaverSafeClass]
public class MyClass
{
    public int Value;
}

public class MyBehaviour : NetworkBehaviour
{
    public void Modify()
    {
        var list = new List<MyClass>();
        list[0].Value = 10;
    }
}
