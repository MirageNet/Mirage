# MIRAGE1206: Invalid RateLimit Attribute Settings

## The Problem
The recognized Mirage `[RateLimit]` on a `[ServerRpc]` contains invalid settings. The Weaver rejects:

- `Interval <= 0`.
- `Refill <= 0` or `MaxTokens <= 0`.
- `Penalty < 0`.

This analyzer contract additionally requires a **finite** interval: `float.NaN` and either infinity are invalid. This is deliberately stricter than the current Weaver's comparison checks. NaN can make the bucket stop rejecting calls, and positive infinity prevents replenishment.

`MaxTokens` may be smaller than `Refill`. The bucket caps replenishment at `MaxTokens`, so this is a valid way to choose a smaller burst capacity. The settings are independent; their ordering is not an error.

The defaults are `Interval = 1f` second, `Refill = 50`, `MaxTokens = 200`, and `Penalty = 1`. `Penalty = 0` is allowed and leaves RPC throttling enabled; it adds no error cost on an excess call. A penalty contributes to the player's separate error-rate-limit handling, rather than specifying a delay in seconds.

RateLimit is consumed only by ServerRpc processing. It does not throttle ordinary methods or ClientRpcs. An unrelated attribute named `RateLimit`, a derived substitute, or a RateLimit placed only on a base declaration is not the recognized attribute on an overriding RPC. This rule validates effective ServerRpc settings; it does not turn those other uses into working limiters.

---

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1206.cs' Name:'mirage1206-triggering' }}}

---

## How to Resolve

Choose a finite positive `Interval`, positive `Refill` and `MaxTokens`, and a non-negative `Penalty`. Set capacity for the permitted initial burst and refill for the intended workload; the defaults are not a security guarantee. See [MIRAGE1207](./MIRAGE1207.md) for scope, host behavior, and failure handling.

{{{ Path:'Snippets/Analyzers/Mirage1206.cs' Name:'mirage1206-resolved' }}}
