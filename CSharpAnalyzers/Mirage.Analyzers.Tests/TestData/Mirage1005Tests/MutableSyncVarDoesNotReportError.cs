using Mirage;

public class Player : NetworkBehaviour
{
    [SyncVar]
    public int health = 100;
}
