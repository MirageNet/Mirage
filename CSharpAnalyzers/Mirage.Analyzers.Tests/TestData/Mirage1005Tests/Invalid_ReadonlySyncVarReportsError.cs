using Mirage;

public class Player : NetworkBehaviour
{
    [SyncVar]
    public readonly int {|#0:health|} = 100;
}
