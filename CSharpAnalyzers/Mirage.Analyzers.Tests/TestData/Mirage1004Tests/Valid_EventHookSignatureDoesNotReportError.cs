using System;
using Mirage;

public class Player : NetworkBehaviour
{
    public event Action<bool> OnReadyChanged;

    [SyncVar(hook = nameof(OnReadyChanged))]
    private bool _isReady;
}
