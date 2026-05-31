using Mirage;

public class MyBehaviour : NetworkBehaviour
{
    [SyncVar]
    public int Health { get; set; }

    private void Start()
    {
        {|#0:Health|} = 100;
    }
}
