using Mirage;

public class Player : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnHealthChanged))]
    public int {|#0:health|};

    // Hook should be void OnHealthChanged(int oldValue, int newValue) or void OnHealthChanged(int newValue)
    public void OnHealthChanged(string wrongType)
    {
    }
}
