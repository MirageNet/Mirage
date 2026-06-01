using Mirage;

public class MyBehaviour : NetworkBehaviour
{
    [SyncVar]
    public int Health { get; set; }

    private void Start()
    {
        Health = 100;
    }
}
