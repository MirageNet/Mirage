using Mirage;

public class MyBehaviour : NetworkBehaviour
{
    private int _mySyncVar;

    [SyncVar]
    public int {|#0:MySyncVar|}
    {
        get => _mySyncVar;
        set => _mySyncVar = value;
    }
}
