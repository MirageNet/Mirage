using Mirage;

public class MyBehaviour : NetworkBehaviour
{
    [SyncVar]
    public int Health;

    private void Start()
    {
        Health = 100;
    }
}
