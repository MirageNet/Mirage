# MIRAGE1203: Static RPC Methods

## The Problem
An RPC method decorated with `[ServerRpc]` or `[ClientRpc]` is declared as `static`.

RPC methods must execute on a specific instance of a `NetworkBehaviour` on a specific `GameObject` so that Mirage knows which network identity the message is targeted at. Static methods lack an instance context (`this`), making it impossible to route the message to the correct network object.

---

## Example of Triggering Code
```csharp
using Mirage;

public class Player : NetworkBehaviour
{
    // Error: ServerRpc method 'CmdSpawnGlobal' must not be static
    [ServerRpc]
    public static void CmdSpawnGlobal()
    {
        // Static context has no NetworkIdentity
    }
}
```

---

## How to Resolve

Remove the `static` modifier from the RPC method declaration so it runs within the instance context of a spawned `NetworkBehaviour`.

```csharp
using Mirage;

public class Player : NetworkBehaviour
{
    // Correct: Instance method has access to the NetworkBehaviour state
    [ServerRpc]
    public void CmdSpawn()
    {
        // Normal instance context
    }
}
```
