using Mirage;

public class Player : NetworkBehaviour
{
    [SyncVar(hook = "SomeNonExistentMethod")]
    public int {|#0:health|};
}
