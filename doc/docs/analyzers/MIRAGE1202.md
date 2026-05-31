# MIRAGE1202: Pass-by-Reference Modifiers in RPCs

## The Problem
An RPC method contains parameters with `ref` or `out` parameter modifiers.

RPCs (Remote Procedure Calls) serialize arguments and send them over the network. Pass-by-reference modifiers (`ref` or `out`) imply that the method can modify the argument and pass the changes back to the caller in-place, which is impossible over a one-way network serialization boundary.

---

## Example of Triggering Code
```csharp
using Mirage;

public class Player : NetworkBehaviour
{
    // Error: ServerRpc method 'CmdTakeDamage' cannot have ref/out parameters
    [ServerRpc]
    public void CmdTakeDamage(ref int health)
    {
        health -= 10;
    }
}
```

---

## How to Resolve

Pass parameters by value. If you need to communicate updated state back to the caller, either use an asynchronous RPC with a `UniTask<T>` return value or update a synchronized property (such as a `[SyncVar]`).

```csharp
using Mirage;

public class Player : NetworkBehaviour
{
    [SyncVar]
    public int Health { get; set; }

    // Correct: Pass by value and synchronize via SyncVar
    [ServerRpc]
    public void CmdTakeDamage(int damage)
    {
        Health -= damage;
    }
}
```
