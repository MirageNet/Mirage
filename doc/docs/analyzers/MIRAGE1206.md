# MIRAGE1206: Missing RateLimit on ServerRpc

## The Problem
A `[ServerRpc]` method is declared without a `[RateLimit]` attribute.

To prevent denial of service (DoS) attacks, server CPU strain, and memory bloat from client RPC spam, it is highly recommended to apply a `[RateLimit]` attribute to every `[ServerRpc]` method. Without a rate limit, a malicious client could flood the server with requests, leading to server performance degradation or player disconnects.

---

## Example of Triggering Code
```csharp
using Mirage;

public class Player : NetworkBehaviour
{
    // Warning: ServerRpc 'CmdFireWeapon' should have a [RateLimit] attribute to prevent spam
    [ServerRpc]
    public void CmdFireWeapon()
    {
    }
}
```

---

## How to Resolve

Add a `[RateLimit]` attribute to the `[ServerRpc]` method with appropriate parameters for the expected rate of call.

```csharp
using Mirage;

public class Player : NetworkBehaviour
{
    // Correct: ServerRpc decorated with [RateLimit] to throttle client requests
    [ServerRpc]
    [RateLimit(Interval = 0.2f, Refill = 5, MaxTokens = 10)]
    public void CmdFireWeapon()
    {
    }
}
```
