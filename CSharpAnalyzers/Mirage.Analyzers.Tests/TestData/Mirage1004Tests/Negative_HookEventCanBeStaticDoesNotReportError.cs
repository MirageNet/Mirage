using System;
using Mirage;

public class Player : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnHealthChanged))]
    public int health;

    public static event Action<int, int> OnHealthChanged;
}
