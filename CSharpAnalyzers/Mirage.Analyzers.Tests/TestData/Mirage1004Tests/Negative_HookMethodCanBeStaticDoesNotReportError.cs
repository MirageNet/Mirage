using Mirage;

public class Player : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnHealthChanged))]
    public int health;

    public static void OnHealthChanged(int oldValue, int newValue)
    {
    }
}
