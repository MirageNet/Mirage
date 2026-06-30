using Mirage;

public class Player : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnHealthChanged))]
    public int {|#0:health|};

    public static void OnHealthChanged(int oldValue, int newValue)
    {
    }
}
