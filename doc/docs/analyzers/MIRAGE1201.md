# MIRAGE1201: RPC Signature Error

## The Problem
A method decorated with `[ServerRpc]` or `[ClientRpc]` violates remote procedure call rules:
1. **Generic Methods:** The method cannot have generic parameters (e.g. `void MyRpc<T>()`).
2. **Invalid Return Type:** The method must return `void`, `UniTask`, or `UniTask<T>`. Returning standard tasks, custom classes, or primitive values is invalid.

Remote procedure calls must serialize their arguments and return values across the network. Non-generic signatures and async return wrappers (`UniTask`) are required for the Mirage Weaver to generate remote execution logic correctly.

---

## Example of Triggering Code
```csharp
using Mirage;
using Cysharp.Threading.Tasks;

public class Player : NetworkBehaviour
{
    // Errors: RPC method 'CmdTakeDamage' is invalid: cannot have generic parameters.
    [ServerRpc]
    public void CmdTakeDamage<T>(T damage)
    {
    }

    // Errors: RPC method 'CmdGetStats' is invalid: cannot return 'PlayerStats'...
    [ServerRpc]
    public PlayerStats CmdGetStats()
    {
        return new PlayerStats();
    }
}
```

---

## How to Resolve

1. Make the method non-generic.
2. Ensure the return type is `void` or a valid async task wrapper like `UniTask` (or `UniTask<T>` for asynchronous RPCs).

```csharp
using Mirage;
using Cysharp.Threading.Tasks;

public class Player : NetworkBehaviour
{
    [ServerRpc]
    public void CmdTakeDamage(int damage)
    {
    }

    [ServerRpc]
    public async UniTask<PlayerStats> CmdGetStats()
    {
        await UniTask.Yield();
        return new PlayerStats();
    }
}
```
