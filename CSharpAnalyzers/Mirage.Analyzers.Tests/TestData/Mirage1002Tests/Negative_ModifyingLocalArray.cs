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
        var array = new MyClass[5];
        array[0].Value = 10;
    }
}
