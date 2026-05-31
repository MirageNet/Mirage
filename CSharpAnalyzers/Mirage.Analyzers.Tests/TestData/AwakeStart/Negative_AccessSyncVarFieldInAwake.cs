using Mirage;

public class MyBehaviour : NetworkBehaviour
{
    [SyncVar]
    public int points { get; set; }

    private void Awake()
    {
        var p = {|#0:points|};
    }
}
