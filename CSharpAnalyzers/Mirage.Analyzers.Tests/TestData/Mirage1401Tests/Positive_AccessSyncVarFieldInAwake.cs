using Mirage;

public class MyBehaviour : NetworkBehaviour
{
    [SyncVar]
    public int points;

    private void Awake()
    {
        var p = points;
    }
}
