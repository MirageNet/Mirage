# MIRAGE1205: Invalid RateLimit Attribute Settings

## The Problem
The `[RateLimit]` attribute contains invalid configurations. This includes:
1. `Interval` is less than or equal to zero.
2. `Refill` is less than or equal to zero.
3. `MaxTokens` is less than or equal to zero, or is less than the `Refill` rate.

Rate limiting buckets require positive numbers for intervals, refill rates, and max tokens to correctly configure token replenishment cycles. If any of these values are zero or negative, or if `MaxTokens` is set to a value less than `Refill`, the rate limiting logic will fail to function or cause infinite loops/resource starvation on the server.

---

## Example of Triggering Code
```csharp
using Mirage;

public class Player : NetworkBehaviour
{
    // Error: RateLimit interval must be greater than zero, and MaxTokens must be >= Refill
    [ServerRpc]
    [RateLimit(Interval = -0.5f, Refill = 10, MaxTokens = 5)]
    public void CmdSpammyAction()
    {
    }
}
```

---

## How to Resolve

Correct the parameters of the `[RateLimit]` attribute to ensure they are positive, valid values. Ensure `MaxTokens` is at least equal to the `Refill` value.

```csharp
using Mirage;

public class Player : NetworkBehaviour
{
    // Correct: Positive interval and MaxTokens >= Refill
    [ServerRpc]
    [RateLimit(Interval = 1.0f, Refill = 10, MaxTokens = 20)]
    public void CmdSpammyAction()
    {
    }
}
```
