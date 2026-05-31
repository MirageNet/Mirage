using Mirage;

public class ValidBehaviour : NetworkBehaviour
{
    [SyncVar]
    public int Health { get; set; }

    [SyncVar]
    public int Points;

    public void Update()
    {
        if (IsServer)
        {
            Health = 100;
            Points = 10;
        }
    }

    public void OnStartServer()
    {
        if (IsClient)
        {
            var h = Health;
        }
    }
}
