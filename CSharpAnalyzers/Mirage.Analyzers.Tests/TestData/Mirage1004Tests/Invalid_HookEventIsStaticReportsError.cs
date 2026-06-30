using System;
using Mirage;

public class Player : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnHealthChanged))]
    public int {|#0:health|};

    public static event Action<int, int> OnHealthChanged;
}
